﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.ML.Data;
using Microsoft.ML.Data.IO;
using Microsoft.ML.Internal.Internallearn;
using Microsoft.ML.Internal.Utilities;
using Microsoft.ML.Runtime;

namespace Microsoft.ML.Model
{
    using ColumnRole = RoleMappedSchema.ColumnRole;
    using Conditional = System.Diagnostics.ConditionalAttribute;

    /// <summary>
    /// This class provides utilities for loading components from the model file generated by MAML commands.
    /// </summary>
    [BestFriend]
    internal static class ModelFileUtils
    {
        public const string DirPredictor = "Predictor";
        public const string DirDataLoaderModel = "DataLoaderModel";
        public const string DirTransformerChain = TransformerChain.LoaderSignature;
        public const string SchemaEntryName = ModelOperationsCatalog.SchemaEntryName;
        // ResultsProcessor needs access to this constant.
        public const string DirTrainingInfo = "TrainingInfo";

        private const string RoleMappingFile = "RoleMapping.txt";

        /// <summary>
        /// Loads and returns the loader and transforms from the specified model stream.
        /// </summary>
        /// <param name="env">The host environment to use.</param>
        /// <param name="modelStream">The model stream.</param>
        /// <param name="files">The data source to initialize the loader with.</param>
        /// <param name="extractInnerPipe">Whether to extract the transforms and loader from the wrapped CompositeDataLoader.</param>
        /// <returns>The created data view.</returns>
        public static IDataView LoadPipeline(IHostEnvironment env, Stream modelStream, IMultiStreamSource files, bool extractInnerPipe = false)
        {
            // REVIEW: Should not duplicate loading loader/transforms code. This method should call LoadLoader.
            Contracts.CheckValue(env, nameof(env));
            env.CheckValue(modelStream, nameof(modelStream));
            env.CheckValue(files, nameof(files));
            using (var rep = RepositoryReader.Open(modelStream, env))
            {
                return LoadPipeline(env, rep, files, extractInnerPipe);
            }
        }

        /// <summary>
        /// Loads and returns the loader and transforms from the specified repository reader.
        /// </summary>
        /// <param name="env">The host environment to use.</param>
        /// <param name="rep">The repository reader.</param>
        /// <param name="files">The data source to initialize the loader with.</param>
        /// <param name="extractInnerPipe">Whether to extract the transforms and loader from the wrapped CompositeDataLoader.</param>
        /// <returns>The created data view.</returns>
        public static IDataView LoadPipeline(IHostEnvironment env, RepositoryReader rep, IMultiStreamSource files, bool extractInnerPipe = false)
        {
            // REVIEW: Should not duplicate loading loader/transforms code. This method should call LoadLoader.
            Contracts.CheckValue(env, nameof(env));
            env.CheckValue(rep, nameof(rep));
            env.CheckValue(files, nameof(files));

            var entry = rep.OpenEntryOrNull(SchemaEntryName);
            if (entry != null)
            {
                var loader = new BinaryLoader(env, new BinaryLoader.Arguments(), entry.Stream);
                ModelLoadContext.LoadModel<ITransformer, SignatureLoadModel>(env, out var transformerChain, rep, DirTransformerChain);
                return transformerChain.Transform(loader);
            }

            using (var ent = rep.OpenEntry(DirDataLoaderModel, ModelLoadContext.ModelStreamName))
            {
                ILegacyDataLoader loader;
                env.Assert(ent.Stream.Position == 0);
                ModelLoadContext.LoadModel<ILegacyDataLoader, SignatureLoadDataLoader>(env, out loader, rep, ent, DirDataLoaderModel, files);
                IDataView result = loader;
                if (extractInnerPipe)
                {
                    var cdl = loader as LegacyCompositeDataLoader;
                    result = cdl == null ? loader : cdl.View;
                }

                return result;
            }
        }

        /// <summary>
        /// Loads all transforms from the model stream, applies them sequentially to the provided data, and returns
        /// the resulting data. If there are no transforms in the stream, or if there's no DataLoader stream at all
        /// (this can happen if the model is produced by old TL), returns the source data.
        /// If the DataLoader stream is invalid, throws.
        /// </summary>
        /// <param name="env">The host environment to use.</param>
        /// <param name="data">The starting data view.</param>
        /// <param name="modelStream">The model stream.</param>
        /// <returns>The resulting data view.</returns>
        public static IDataView LoadTransforms(IHostEnvironment env, IDataView data, Stream modelStream)
        {
            // REVIEW: Consolidate with LoadTransformChain in DataDiagnosticsCommand.
            Contracts.CheckValue(env, nameof(env));
            env.CheckValue(data, nameof(data));
            env.CheckValue(modelStream, nameof(modelStream));
            using (var rep = RepositoryReader.Open(modelStream, env))
            {
                return LoadTransforms(env, data, rep);
            }
        }

        /// <summary>
        /// Loads all transforms from the model stream, applies them sequentially to the provided data, and returns
        /// the resulting data. If there are no transforms in the stream, or if there's no DataLoader stream at all
        /// (this can happen if the model is produced by old TL), returns the source data.
        /// If the DataLoader stream is invalid, throws.
        /// </summary>
        /// <param name="env">The host environment to use.</param>
        /// <param name="data">The starting data view.</param>
        /// <param name="rep">The repository reader.</param>
        /// <returns>The resulting data view.</returns>
        public static IDataView LoadTransforms(IHostEnvironment env, IDataView data, RepositoryReader rep)
        {
            Contracts.CheckValue(env, nameof(env));
            env.CheckValue(data, nameof(data));
            env.CheckValue(rep, nameof(rep));
            using (var ent = rep.OpenEntryOrNull(DirDataLoaderModel, ModelLoadContext.ModelStreamName))
            {
                if (ent == null)
                    return data;
                var ctx = new ModelLoadContext(rep, ent, DirDataLoaderModel);
                return LegacyCompositeDataLoader.LoadSelectedTransforms(ctx, data, env, x => true);
            }
        }

        /// <summary>
        /// Loads a predictor from the model stream. Returns null iff there's no predictor.
        /// </summary>
        public static IPredictor LoadPredictorOrNull(IHostEnvironment env, Stream modelStream)
        {
            Contracts.CheckValue(env, nameof(env));
            Contracts.CheckValue(modelStream, nameof(modelStream));
            using (var rep = RepositoryReader.Open(modelStream, env))
                return LoadPredictorOrNull(env, rep);
        }

        /// <summary>
        /// Loads a predictor from the repository. Returns null iff there's no predictor.
        /// </summary>
        public static IPredictor LoadPredictorOrNull(IHostEnvironment env, RepositoryReader rep)
        {
            Contracts.CheckValue(env, nameof(env));
            Contracts.CheckValue(rep, nameof(rep));
            IPredictor predictor;
            ModelLoadContext.LoadModelOrNull<IPredictor, SignatureLoadModel>(env, out predictor, rep, DirPredictor);
            return predictor;
        }

        /// <summary>
        /// Given a repository, returns the save context for saving the data loader model.
        /// </summary>
        public static ModelSaveContext GetDataModelSavingContext(RepositoryWriter rep)
        {
            Contracts.CheckValue(rep, nameof(rep));
            return new ModelSaveContext(rep, DirDataLoaderModel, ModelLoadContext.ModelStreamName);
        }

        /// <summary>
        /// Loads data view (loader and transforms) from <paramref name="rep"/> if <paramref name="loadTransforms"/> is set to true,
        /// otherwise loads loader only.
        /// </summary>
        public static ILegacyDataLoader LoadLoader(IHostEnvironment env, RepositoryReader rep, IMultiStreamSource files, bool loadTransforms)
        {
            Contracts.CheckValue(env, nameof(env));
            env.CheckValue(rep, nameof(rep));
            env.CheckValue(files, nameof(files));

            ILegacyDataLoader loader;

            // If loadTransforms is false, load the loader only, not the transforms.
            Repository.Entry ent = null;
            string dir = "";
            if (!loadTransforms)
                ent = rep.OpenEntryOrNull(dir = Path.Combine(DirDataLoaderModel, "Loader"), ModelLoadContext.ModelStreamName);

            if (ent == null) // either loadTransforms is true, or it's not a composite loader
                ent = rep.OpenEntry(dir = DirDataLoaderModel, ModelLoadContext.ModelStreamName);

            env.CheckDecode(ent != null, "Loader is not found.");
            env.AssertNonEmpty(dir);
            using (ent)
            {
                env.Assert(ent.Stream.Position == 0);
                ModelLoadContext.LoadModel<ILegacyDataLoader, SignatureLoadDataLoader>(env, out loader, rep, ent, dir, files);
            }
            return loader;
        }

        /// <summary>
        /// REVIEW: consider adding an overload that returns <see cref="ReadOnlyMemory{T}"/> of <see cref="char"/>
        /// Loads optionally feature names from the repository directory.
        /// Returns false iff no stream was found for feature names, iff result is set to null.
        /// </summary>
        public static bool TryLoadFeatureNames(out FeatureNameCollection featureNames, RepositoryReader rep)
        {
            Contracts.CheckValue(rep, nameof(rep));

            using (var ent = rep.OpenEntryOrNull(ModelFileUtils.DirTrainingInfo, "FeatureNames.bin"))
            {
                if (ent != null)
                {
                    using (var ctx = new ModelLoadContext(rep, ent, ModelFileUtils.DirTrainingInfo))
                    {
                        featureNames = FeatureNameCollection.Create(ctx);
                        return true;
                    }
                }
            }

            featureNames = null;
            return false;
        }

        /// <summary>
        /// Save schema associations of role/column-name in <paramref name="rep"/>.
        /// </summary>
        internal static void SaveRoleMappings(IHostEnvironment env, IChannel ch, RoleMappedSchema schema, RepositoryWriter rep)
        {
            // REVIEW: Should we also save this stuff, for instance, in some portion of the
            // score command or transform?
            Contracts.AssertValue(env);
            env.AssertValue(ch);
            ch.AssertValue(schema);

            ArrayDataViewBuilder builder = new ArrayDataViewBuilder(env);

            List<string> rolesList = new List<string>();
            List<string> columnNamesList = new List<string>();
            // OrderBy is stable, so there is no danger in it "reordering" columns
            // when a role is filled by multiple columns.
            foreach (var role in schema.GetColumnRoleNames().OrderBy(r => r.Key.Value))
            {
                rolesList.Add(role.Key.Value);
                columnNamesList.Add(role.Value);
            }
            builder.AddColumn("Role", rolesList.ToArray());
            builder.AddColumn("Column", columnNamesList.ToArray());

            using (var entry = rep.CreateEntry(DirTrainingInfo, RoleMappingFile))
            {
                // REVIEW: It seems very important that we have the role mappings
                // be easily human interpretable and even manipulable, but relying on the
                // text saver/loader means that special characters like '\n' won't be reinterpretable.
                // On the other hand, no one is such a big lunatic that they will actually
                // ever go ahead and do something so stupid as that.
                var saver = new TextSaver(env, new TextSaver.Arguments() { Dense = true, Silent = true });
                var view = builder.GetDataView();
                saver.SaveData(entry.Stream, view, Utils.GetIdentityPermutation(view.Schema.Count));
            }
        }

        /// <summary>
        /// Return role/column-name pairs loaded from <paramref name="modelStream"/>.
        /// </summary>
        public static IEnumerable<KeyValuePair<ColumnRole, string>> LoadRoleMappingsOrNull(IHostEnvironment env, Stream modelStream)
        {
            Contracts.CheckValue(env, nameof(env));
            env.CheckValue(modelStream, nameof(modelStream));
            using (var rep = RepositoryReader.Open(modelStream, env))
            {
                return LoadRoleMappingsOrNull(env, rep);
            }
        }

        /// <summary>
        /// Return role/column-name pairs loaded from a repository.
        /// </summary>
        public static IEnumerable<KeyValuePair<ColumnRole, string>> LoadRoleMappingsOrNull(IHostEnvironment env, RepositoryReader rep)
        {
            Contracts.CheckValue(env, nameof(env));
            var h = env.Register("RoleMappingUtils");

            var list = new List<KeyValuePair<string, string>>();

            var entry = rep.OpenEntryOrNull(DirTrainingInfo, RoleMappingFile);
            if (entry == null)
                return null;
            entry.Dispose();

            using (var ch = h.Start("Loading role mappings"))
            {
                // REVIEW: Should really validate the schema here, and consider
                // ignoring this stream if it isn't as expected.
                var repoStreamWrapper = new RepositoryStreamWrapper(rep, DirTrainingInfo, RoleMappingFile);
                var loader = new TextLoader(env, dataSample: repoStreamWrapper).Load(repoStreamWrapper);

                using (var cursor = loader.GetRowCursorForAllColumns())
                {
                    var roleGetter = cursor.GetGetter<ReadOnlyMemory<char>>(cursor.Schema[0]);
                    var colGetter = cursor.GetGetter<ReadOnlyMemory<char>>(cursor.Schema[1]);
                    var role = default(ReadOnlyMemory<char>);
                    var col = default(ReadOnlyMemory<char>);
                    while (cursor.MoveNext())
                    {
                        roleGetter(ref role);
                        colGetter(ref col);
                        string roleStr = role.ToString();
                        string colStr = col.ToString();

                        h.CheckDecode(!string.IsNullOrWhiteSpace(roleStr), "Role name must not be empty");
                        h.CheckDecode(!string.IsNullOrWhiteSpace(colStr), "Column name must not be empty");
                        list.Add(new KeyValuePair<string, string>(roleStr, colStr));
                    }
                }
            }

            return TrainUtils.CheckAndGenerateCustomColumns(env, list.ToArray());
        }

        /// <summary>
        /// Returns the <see cref="RoleMappedSchema"/> from a model stream, or <c>null</c> if there were no
        /// role mappings present.
        /// </summary>
        public static RoleMappedSchema LoadRoleMappedSchemaOrNull(IHostEnvironment env, Stream modelStream)
        {
            Contracts.CheckValue(env, nameof(env));
            env.CheckValue(modelStream, nameof(modelStream));
            using (var rep = RepositoryReader.Open(modelStream, env))
            {
                return LoadRoleMappedSchemaOrNull(env, rep);
            }
        }

        /// <summary>
        /// Returns the <see cref="RoleMappedSchema"/> from a repository, or <c>null</c> if there were no
        /// role mappings present.
        /// </summary>
        public static RoleMappedSchema LoadRoleMappedSchemaOrNull(IHostEnvironment env, RepositoryReader rep)
        {
            Contracts.CheckValue(env, nameof(env));
            var h = env.Register("RoleMappingUtils");

            var roleMappings = ModelFileUtils.LoadRoleMappingsOrNull(env, rep);
            if (roleMappings == null)
                return null;
            var pipe = ModelFileUtils.LoadLoader(h, rep, new MultiFileSource(null), loadTransforms: true);
            return new RoleMappedSchema(pipe.Schema, roleMappings);
        }

        /// <summary>
        /// The RepositoryStreamWrapper is a IMultiStreamSource wrapper of a Stream object in a repository.
        /// It is used to deserialize RoleMappings.txt from a model zip file.
        /// </summary>
        private sealed class RepositoryStreamWrapper : IMultiStreamSource
        {
            private readonly RepositoryReader _repository;
            private readonly string _directory;
            private readonly string _filename;

            public RepositoryStreamWrapper(RepositoryReader repository, string directory, string filename)
            {
                Contracts.CheckValue(repository, nameof(repository));
                Contracts.CheckNonWhiteSpace(directory, nameof(directory));
                Contracts.CheckNonWhiteSpace(filename, nameof(filename));

                _repository = repository;
                _directory = directory;
                _filename = filename;
            }

            public int Count { get { return 1; } }

            public string GetPathOrNull(int index)
            {
                Contracts.Check(index == 0);
                return null;
            }

            public Stream Open(int index)
            {
                Contracts.Assert(index == 0);
                var ent = _repository.OpenEntryOrNull(_directory, _filename);
                if (ent == null)
                    throw Contracts.Except($"File '{_filename}' is missing from the repository");
                return new EntryStream(ent);
            }

            public TextReader OpenTextReader(int index) { return new StreamReader(Open(index)); }

            /// <summary>
            /// A custom entry stream wrapper that includes custom dispose logic for disposing the entry
            /// when the stream is disposed.
            /// </summary>
            private sealed class EntryStream : Stream
            {
                private bool _disposed;

                private readonly Repository.Entry _entry;

                public override bool CanRead
                {
                    get
                    {
                        AssertValid();
                        return _entry.Stream.CanRead;
                    }
                }

                public override bool CanSeek
                {
                    get
                    {
                        AssertValid();
                        return _entry.Stream.CanSeek;
                    }
                }

                public override bool CanWrite
                {
                    get
                    {
                        AssertValid();
                        return _entry.Stream.CanWrite;
                    }
                }

                public override long Length
                {
                    get
                    {
                        AssertValid();
                        return _entry.Stream.Length;
                    }
                }

                public override long Position
                {
                    get
                    {
                        AssertValid();
                        return _entry.Stream.Position;
                    }

                    set
                    {
                        AssertValid();
                        _entry.Stream.Position = value;
                    }
                }

                public EntryStream(Repository.Entry entry)
                {
                    Contracts.CheckValue(entry, nameof(entry));
                    Contracts.CheckValue(entry.Stream, nameof(entry.Stream));
                    _entry = entry;
                }

                public override void Flush()
                {
                    AssertValid();
                    _entry.Stream.Flush();
                }

                public override long Seek(long offset, SeekOrigin origin)
                {
                    AssertValid();
                    return _entry.Stream.Seek(offset, origin);
                }

                public override void SetLength(long value)
                {
                    AssertValid();
                    _entry.Stream.SetLength(value);
                }

                public override int Read(byte[] buffer, int offset, int count)
                {
                    AssertValid();
                    return _entry.Stream.Read(buffer, offset, count);
                }

                public override void Write(byte[] buffer, int offset, int count)
                {
                    AssertValid();
                    _entry.Stream.Write(buffer, offset, count);
                }

                protected override void Dispose(bool disposing)
                {
                    if (!_disposed)
                    {
                        AssertValid();
                        _entry.Dispose();
                        _disposed = true;
                    }
                    base.Dispose(disposing);
                }

                [Conditional("DEBUG")]
                private void AssertValid()
                {
#if DEBUG
                    Contracts.AssertValue(_entry);
                    Contracts.AssertValue(_entry.Stream);
#endif
                }
            }
        }
    }
}