﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.ML;
using Microsoft.ML.CommandLine;
using Microsoft.ML.Data;
using Microsoft.ML.Internal.Internallearn;
using Microsoft.ML.Internal.Utilities;
using Microsoft.ML.Runtime;
using Microsoft.ML.Transforms.Image;

[assembly: LoadableClass(ImageResizingTransformer.Summary, typeof(IDataTransform), typeof(ImageResizingTransformer), typeof(ImageResizingTransformer.Arguments),
    typeof(SignatureDataTransform), ImageResizingTransformer.UserName, "ImageResizerTransform", "ImageResizer")]

[assembly: LoadableClass(ImageResizingTransformer.Summary, typeof(IDataTransform), typeof(ImageResizingTransformer), null, typeof(SignatureLoadDataTransform),
    ImageResizingTransformer.UserName, ImageResizingTransformer.LoaderSignature)]

[assembly: LoadableClass(typeof(ImageResizingTransformer), null, typeof(SignatureLoadModel),
    ImageResizingTransformer.UserName, ImageResizingTransformer.LoaderSignature)]

[assembly: LoadableClass(typeof(IRowMapper), typeof(ImageResizingTransformer), null, typeof(SignatureLoadRowMapper),
    ImageResizingTransformer.UserName, ImageResizingTransformer.LoaderSignature)]

namespace Microsoft.ML.Transforms.Image
{
    // REVIEW: Rewrite as LambdaTransform to simplify.
    /// <summary>
    /// <see cref="ITransformer"/> resulting from fitting an <see cref="ImageResizingEstimator"/>.
    /// </summary>
    public sealed class ImageResizingTransformer : OneToOneTransformerBase
    {
        internal sealed class Column : OneToOneColumn
        {
            [Argument(ArgumentType.AtMostOnce, HelpText = "Width of the resized image", ShortName = "width")]
            public int? ImageWidth;

            [Argument(ArgumentType.AtMostOnce, HelpText = "Height of the resized image", ShortName = "height")]
            public int? ImageHeight;

            [Argument(ArgumentType.AtMostOnce, HelpText = "Resizing method", ShortName = "scale")]
            public ImageResizingEstimator.ResizingKind? Resizing;

            [Argument(ArgumentType.AtMostOnce, HelpText = "Anchor for cropping", ShortName = "anchor")]
            public ImageResizingEstimator.Anchor? CropAnchor;

            internal static Column Parse(string str)
            {
                Contracts.AssertNonEmpty(str);

                var res = new Column();
                if (res.TryParse(str))
                    return res;
                return null;
            }

            internal bool TryUnparse(StringBuilder sb)
            {
                Contracts.AssertValue(sb);
                if (ImageWidth != null || ImageHeight != null || Resizing != null || CropAnchor != null)
                    return false;
                return TryUnparseCore(sb);
            }
        }

        internal class Arguments : TransformInputBase
        {
            [Argument(ArgumentType.Multiple | ArgumentType.Required, HelpText = "New column definition(s) (optional form: name:src)", Name = "Column", ShortName = "col", SortOrder = 1)]
            public Column[] Columns;

            [Argument(ArgumentType.Required, HelpText = "Resized width of the image", ShortName = "width")]
            public int ImageWidth;

            [Argument(ArgumentType.Required, HelpText = "Resized height of the image", ShortName = "height")]
            public int ImageHeight;

            [Argument(ArgumentType.AtMostOnce, HelpText = "Resizing method", ShortName = "scale")]
            public ImageResizingEstimator.ResizingKind Resizing = ImageResizingEstimator.Defaults.Resizing;

            [Argument(ArgumentType.AtMostOnce, HelpText = "Anchor for cropping", ShortName = "anchor")]
            public ImageResizingEstimator.Anchor CropAnchor = ImageResizingEstimator.Defaults.CropAnchor;
        }

        internal const string Summary = "Scales an image to specified dimensions using one of the three scale types: isotropic with padding, "
            + "isotropic with cropping or anisotropic. In case of isotropic padding, transparent color is used to pad resulting image.";

        internal const string UserName = "Image Resizer Transform";
        internal const string LoaderSignature = "ImageScalerTransform";
        private static VersionInfo GetVersionInfo()
        {
            return new VersionInfo(
                modelSignature: "IMGSCALF",
                //verWrittenCur: 0x00010001, // Initial
                //verWrittenCur: 0x00010002, // Swith from OpenCV to Bitmap
                verWrittenCur: 0x00010003, // No more sizeof(float)
                verReadableCur: 0x00010003,
                verWeCanReadBack: 0x00010003,
                loaderSignature: LoaderSignature,
                loaderAssemblyName: typeof(ImageResizingTransformer).Assembly.FullName);
        }

        private const string RegistrationName = "ImageScaler";

        private readonly ImageResizingEstimator.ColumnOptions[] _columns;

        /// <summary>
        /// The columns passed to this <see cref="ITransformer"/>.
        /// </summary>
        internal IReadOnlyCollection<ImageResizingEstimator.ColumnOptions> Columns => _columns.AsReadOnly();

        ///<summary>
        /// Resize image.
        ///</summary>
        /// <param name="env">The host environment.</param>
        /// <param name="outputColumnName">Name of the column resulting from the transformation of <paramref name="inputColumnName"/>.</param>
        /// <param name="imageWidth">Width of resized image.</param>
        /// <param name="imageHeight">Height of resized image.</param>
        /// <param name="inputColumnName">Name of the input column.</param>
        /// <param name="resizing">What <see cref="ImageResizingEstimator.ResizingKind"/> to use.</param>
        /// <param name="cropAnchor">If <paramref name="resizing"/> set to <see cref="ImageResizingEstimator.ResizingKind.IsoCrop"/> what anchor to use for cropping.</param>
        internal ImageResizingTransformer(IHostEnvironment env, string outputColumnName,
            int imageWidth, int imageHeight, string inputColumnName = null,
            ImageResizingEstimator.ResizingKind resizing = ImageResizingEstimator.ResizingKind.IsoCrop,
            ImageResizingEstimator.Anchor cropAnchor = ImageResizingEstimator.Anchor.Center)
            : this(env, new ImageResizingEstimator.ColumnOptions(outputColumnName, imageWidth, imageHeight, inputColumnName, resizing, cropAnchor))
        {
        }

        ///<summary>
        /// Resize image.
        ///</summary>
        /// <param name="env">The host environment.</param>
        /// <param name="columns">Describes the parameters of image resizing for each column pair.</param>
        internal ImageResizingTransformer(IHostEnvironment env, params ImageResizingEstimator.ColumnOptions[] columns)
            : base(Contracts.CheckRef(env, nameof(env)).Register(RegistrationName), GetColumnPairs(columns))
        {
            _columns = columns.ToArray();
        }

        private static (string outputColumnName, string inputColumnName)[] GetColumnPairs(ImageResizingEstimator.ColumnOptions[] columns)
        {
            Contracts.CheckValue(columns, nameof(columns));
            return columns.Select(x => (x.Name, x.InputColumnName)).ToArray();
        }

        // Factory method for SignatureDataTransform.
        internal static IDataTransform Create(IHostEnvironment env, Arguments args, IDataView input)
        {
            Contracts.CheckValue(env, nameof(env));
            env.CheckValue(args, nameof(args));
            env.CheckValue(input, nameof(input));

            env.CheckValue(args.Columns, nameof(args.Columns));

            var cols = new ImageResizingEstimator.ColumnOptions[args.Columns.Length];
            for (int i = 0; i < cols.Length; i++)
            {
                var item = args.Columns[i];
                cols[i] = new ImageResizingEstimator.ColumnOptions(
                    item.Name,
                    item.ImageWidth ?? args.ImageWidth,
                    item.ImageHeight ?? args.ImageHeight,
                    item.Source ?? item.Name,
                    item.Resizing ?? args.Resizing,
                    item.CropAnchor ?? args.CropAnchor);
            }

            return new ImageResizingTransformer(env, cols).MakeDataTransform(input);
        }

        // Factory method for SignatureLoadModel.
        private static ImageResizingTransformer Create(IHostEnvironment env, ModelLoadContext ctx)
        {
            Contracts.CheckValue(env, nameof(env));
            var host = env.Register(RegistrationName);

            host.CheckValue(ctx, nameof(ctx));
            ctx.CheckAtModel(GetVersionInfo());

            return new ImageResizingTransformer(host, ctx);
        }

        private ImageResizingTransformer(IHost host, ModelLoadContext ctx)
            : base(host, ctx)
        {
            // *** Binary format ***
            // <base>

            // for each added column
            //   int: width
            //   int: height
            //   byte: scaling kind
            //   byte: anchor

            _columns = new ImageResizingEstimator.ColumnOptions[ColumnPairs.Length];
            for (int i = 0; i < ColumnPairs.Length; i++)
            {
                int width = ctx.Reader.ReadInt32();
                Host.CheckDecode(width > 0);
                int height = ctx.Reader.ReadInt32();
                Host.CheckDecode(height > 0);
                var scale = (ImageResizingEstimator.ResizingKind)ctx.Reader.ReadByte();
                Host.CheckDecode(Enum.IsDefined(typeof(ImageResizingEstimator.ResizingKind), scale));
                var anchor = (ImageResizingEstimator.Anchor)ctx.Reader.ReadByte();
                Host.CheckDecode(Enum.IsDefined(typeof(ImageResizingEstimator.Anchor), anchor));
                _columns[i] = new ImageResizingEstimator.ColumnOptions(ColumnPairs[i].outputColumnName, width, height, ColumnPairs[i].inputColumnName, scale, anchor);
            }
        }

        // Factory method for SignatureLoadDataTransform.
        private static IDataTransform Create(IHostEnvironment env, ModelLoadContext ctx, IDataView input)
            => Create(env, ctx).MakeDataTransform(input);

        // Factory method for SignatureLoadRowMapper.
        private static IRowMapper Create(IHostEnvironment env, ModelLoadContext ctx, DataViewSchema inputSchema)
            => Create(env, ctx).MakeRowMapper(inputSchema);

        private protected override void SaveModel(ModelSaveContext ctx)
        {
            Host.CheckValue(ctx, nameof(ctx));

            ctx.CheckAtModel();
            ctx.SetVersionInfo(GetVersionInfo());

            // *** Binary format ***
            // <base>

            // for each added column
            //   int: width
            //   int: height
            //   byte: scaling kind
            //   byte: anchor

            base.SaveColumns(ctx);

            foreach (var col in _columns)
            {
                ctx.Writer.Write(col.ImageWidth);
                ctx.Writer.Write(col.ImageHeight);
                Contracts.Assert((ImageResizingEstimator.ResizingKind)(byte)col.Resizing == col.Resizing);
                ctx.Writer.Write((byte)col.Resizing);
                Contracts.Assert((ImageResizingEstimator.Anchor)(byte)col.Anchor == col.Anchor);
                ctx.Writer.Write((byte)col.Anchor);
            }
        }

        private protected override IRowMapper MakeRowMapper(DataViewSchema schema) => new Mapper(this, schema);

        private protected override void CheckInputColumn(DataViewSchema inputSchema, int col, int srcCol)
        {
            if (!(inputSchema[srcCol].Type is ImageDataViewType))
                throw Host.ExceptSchemaMismatch(nameof(inputSchema), "input", _columns[col].InputColumnName, "image", inputSchema[srcCol].Type.ToString());
        }

        private sealed class Mapper : OneToOneMapperBase
        {
            private readonly ImageResizingTransformer _parent;

            public Mapper(ImageResizingTransformer parent, DataViewSchema inputSchema)
                : base(parent.Host.Register(nameof(Mapper)), parent, inputSchema)
            {
                _parent = parent;
            }

            protected override DataViewSchema.DetachedColumn[] GetOutputColumnsCore()
                => _parent._columns.Select(x => new DataViewSchema.DetachedColumn(x.Name, x.Type, null)).ToArray();

            protected override Delegate MakeGetter(DataViewRow input, int iinfo, Func<int, bool> activeOutput, out Action disposer)
            {
                Contracts.AssertValue(input);
                Contracts.Assert(0 <= iinfo && iinfo < _parent._columns.Length);

                var src = default(MLImage);
                var getSrc = input.GetGetter<MLImage>(input.Schema[ColMapNewToOld[iinfo]]);
                var info = _parent._columns[iinfo];

                disposer =
                    () =>
                    {
                        if (src != null)
                        {
                            src = null;
                        }
                    };

                ValueGetter<MLImage> del =
                    (ref MLImage dst) =>
                    {
                        if (dst != null)
                        {
                            dst.Dispose();
                        }

                        getSrc(ref src);
                        if (src == null || src.Height <= 0 || src.Width <= 0)
                            return;
                        if (src.Height == info.ImageHeight && src.Width == info.ImageWidth)
                        {
                            dst = src;
                            return;
                        }

                        dst = src.CloneWithResizing(
                                info.ImageWidth,
                                info.ImageHeight,
                                info.Resizing switch
                                {
                                    ImageResizingEstimator.ResizingKind.IsoPad => ImageResizeMode.Pad,
                                    ImageResizingEstimator.ResizingKind.IsoCrop =>
                                        info.Anchor switch
                                        {
                                            ImageResizingEstimator.Anchor.Top => ImageResizeMode.CropAnchorTop,
                                            ImageResizingEstimator.Anchor.Bottom => ImageResizeMode.CropAnchorBottom,
                                            ImageResizingEstimator.Anchor.Left => ImageResizeMode.CropAnchorLeft,
                                            ImageResizingEstimator.Anchor.Right => ImageResizeMode.CropAnchorRight,
                                            _ => ImageResizeMode.CropAnchorCentral
                                        },
                                    ImageResizingEstimator.ResizingKind.Fill => ImageResizeMode.Fill,
                                    _ => throw new InvalidOperationException($"Invalid image resizing mode value")
                                });

                        dst.Tag = src.Tag;
                        Contracts.Assert(dst.Width == info.ImageWidth && dst.Height == info.ImageHeight);
                    };

                return del;
            }
        }
    }

    /// <summary>
    /// <see cref="IEstimator{TTransformer}"/> for the <see cref="ImageResizingTransformer"/>.
    /// </summary>
    /// <remarks>
    /// <format type="text/markdown"><![CDATA[
    ///
    /// ###  Estimator Characteristics
    /// |  |  |
    /// | -- | -- |
    /// | Does this estimator need to look at the data to train its parameters? | No |
    /// | Input column data type | <xref:Microsoft.ML.Data.MLImage> |
    /// | Output column data type | <xref:Microsoft.ML.Data.MLImage> |
    /// | Required NuGet in addition to Microsoft.ML | Microsoft.ML.ImageAnalytics |
    /// | Exportable to ONNX | No |
    ///
    /// The resulting <xref:Microsoft.ML.Transforms.Image.ImageResizingTransformer> creates a new column, named as specified in the output column name parameters, and
    /// resizes the data from the input column to this new column.
    ///
    /// In image processing pipelines, often machine learning practitioner make use of
    /// [pre-trained DNN featurizers](https://blogs.msdn.microsoft.com/mlserver/2017/04/12/image-featurization-with-a-pre-trained-deep-neural-network-model) to extract features for usage in the machine learning algorithms.
    /// Those pre-trained models have a defined width and height for their input images, so often, after getting loaded, the images will need to get resized before
    /// further processing.
    /// For end-to-end image processing pipelines, and scenarios in your applications, see the
    /// [examples](https://github.com/dotnet/machinelearning-samples/tree/main/samples/csharp/getting-started) in the machinelearning-samples github repository.
    ///
    /// Check the See Also section for links to usage examples.
    /// ]]>
    /// </format>
    /// </remarks>
    /// <seealso cref="ImageEstimatorsCatalog.ResizeImages(TransformsCatalog, string, int, int, string, ResizingKind, Anchor)"/>
    public sealed class ImageResizingEstimator : TrivialEstimator<ImageResizingTransformer>
    {
        internal static class Defaults
        {
            public const ResizingKind Resizing = ResizingKind.IsoCrop;
            public const Anchor CropAnchor = Anchor.Center;
        }

        /// <summary>
        /// Specifies how to resize the images: by cropping them or padding in the direction needed to fill up.
        /// </summary>
        public enum ResizingKind : byte
        {
            /// <summary>
            /// Isotropic(uniform) with padding.
            /// </summary>
            [TGUI(Label = "Isotropic with Padding")]
            IsoPad = 0,

            /// <summary>
            /// Isotropic(uniform) with cropping.
            /// </summary>
            [TGUI(Label = "Isotropic with Cropping")]
            IsoCrop = 1,

            /// <summary>
            /// Ignore aspect ratio and squeeze/stretch into target dimensions.
            /// </summary>
            [TGUI(Label = "Ignore aspect ratio and squeeze/stretch into target dimensions")]
            Fill = 2
        }

        /// <summary>
        /// Indicates where to anchor for image cropping, for example set to <see cref="Center"/> is going to crop the image on all sides,
        /// set to <see cref="Bottom"/> is going to crop the image at the top.
        /// </summary>
        public enum Anchor : byte
        {
            Right = 0,
            Left = 1,
            Top = 2,
            Bottom = 3,
            Center = 4
        }

        /// <summary>
        /// Describes how the transformer handles one image resize column.
        /// </summary>
        [BestFriend]
        internal sealed class ColumnOptions
        {
            /// <summary>Name of the column resulting from the transformation of <see cref="InputColumnName"/></summary>
            public readonly string Name;

            /// <summary>Name of column to transform.</summary>
            public readonly string InputColumnName;

            /// <summary>Width to resize the image to.</summary>
            public readonly int ImageWidth;

            /// <summary>Height to resize the image to.</summary>
            public readonly int ImageHeight;

            /// <summary>What <see cref="ResizingKind"/> to use (uniform, or non-uniform).</summary>
            public readonly ResizingKind Resizing;

            /// <summary>If <see cref="Resizing"/> set to <see cref="ResizingKind.IsoCrop"/> what anchor to use for cropping.</summary>
            public readonly Anchor Anchor;

            /// <summary>The type of column, <see cref="DataViewType"/>.</summary>
            public readonly DataViewType Type;

            /// <summary>
            /// Describes how the transformer handles one image resize column pair.
            /// </summary>
            /// <param name="name">Name of the column resulting from the transformation of <paramref name="inputColumnName"/>.</param>
            /// <param name="imageWidth">Width of resized image.</param>
            /// <param name="imageHeight">Height of resized image.</param>
            /// <param name="inputColumnName">Name of column to transform. If set to <see langword="null"/>, the value of the <paramref name="name"/> will be used as source.</param>
            /// <param name="resizing">What <see cref="ImageResizingEstimator.ResizingKind"/> to use.</param>
            /// <param name="anchor">If <paramref name="resizing"/> set to <see cref="ImageResizingEstimator.ResizingKind.IsoCrop"/> what anchor to use for cropping.</param>
            public ColumnOptions(string name,
                int imageWidth,
                int imageHeight,
                string inputColumnName = null,
                ResizingKind resizing = Defaults.Resizing,
                Anchor anchor = Defaults.CropAnchor)
            {
                Contracts.CheckNonEmpty(name, nameof(name));
                Contracts.CheckUserArg(imageWidth > 0, nameof(imageWidth));
                Contracts.CheckUserArg(imageHeight > 0, nameof(imageHeight));
                Contracts.CheckUserArg(Enum.IsDefined(typeof(ResizingKind), resizing), nameof(resizing));
                Contracts.CheckUserArg(Enum.IsDefined(typeof(Anchor), anchor), nameof(anchor));

                Name = name;
                InputColumnName = inputColumnName ?? name;
                ImageWidth = imageWidth;
                ImageHeight = imageHeight;
                Resizing = resizing;
                Anchor = anchor;
                Type = new ImageDataViewType(ImageHeight, ImageWidth);
            }
        }

        /// <summary>
        /// Estimator which resizes images.
        /// </summary>
        /// <param name="env">The host environment.</param>
        /// <param name="outputColumnName">Name of the column resulting from the transformation of <paramref name="inputColumnName"/>.</param>
        /// <param name="imageWidth">Width of resized image.</param>
        /// <param name="imageHeight">Height of resized image.</param>
        /// <param name="inputColumnName">Name of the input column.</param>
        /// <param name="resizing">What <see cref="ResizingKind"/> to use.</param>
        /// <param name="cropAnchor">If <paramref name="resizing"/> set to <see cref="ResizingKind.IsoCrop"/> what anchor to use for cropping.</param>
        internal ImageResizingEstimator(IHostEnvironment env,
            string outputColumnName,
            int imageWidth,
            int imageHeight,
            string inputColumnName = null,
            ResizingKind resizing = Defaults.Resizing,
            Anchor cropAnchor = Defaults.CropAnchor)
            : this(env, new ImageResizingTransformer(env, outputColumnName, imageWidth, imageHeight, inputColumnName, resizing, cropAnchor))
        {
        }

        /// <summary>
        /// Estimator which resizes images.
        /// </summary>
        /// <param name="env">The host environment.</param>
        /// <param name="columns">Describes the parameters of image resizing for each column pair.</param>
        internal ImageResizingEstimator(IHostEnvironment env, params ColumnOptions[] columns)
            : this(env, new ImageResizingTransformer(env, columns))
        {
        }

        private ImageResizingEstimator(IHostEnvironment env, ImageResizingTransformer transformer)
            : base(Contracts.CheckRef(env, nameof(env)).Register(nameof(ImageResizingEstimator)), transformer)
        {
        }

        /// <summary>
        /// Returns the <see cref="SchemaShape"/> of the schema which will be produced by the transformer.
        /// Used for schema propagation and verification in a pipeline.
        /// </summary>
        public override SchemaShape GetOutputSchema(SchemaShape inputSchema)
        {
            Host.CheckValue(inputSchema, nameof(inputSchema));
            var result = inputSchema.ToDictionary(x => x.Name);
            foreach (var colInfo in Transformer.Columns)
            {
                if (!inputSchema.TryFindColumn(colInfo.InputColumnName, out var col))
                    throw Host.ExceptSchemaMismatch(nameof(inputSchema), "input", colInfo.InputColumnName);
                if (!(col.ItemType is ImageDataViewType) || col.Kind != SchemaShape.Column.VectorKind.Scalar)
                    throw Host.ExceptSchemaMismatch(nameof(inputSchema), "input", colInfo.InputColumnName, new ImageDataViewType().ToString(), col.GetTypeString());

                result[colInfo.Name] = new SchemaShape.Column(colInfo.Name, SchemaShape.Column.VectorKind.Scalar, colInfo.Type, false);
            }

            return new SchemaShape(result.Values);
        }
    }
}
