﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.ML.Internal.Utilities;
using Microsoft.ML.Runtime;

namespace Microsoft.ML.Data
{
    /// <summary>
    /// Wraps a potentially compound path as an IMultiStreamSource.
    /// </summary>
    /// <remarks>Expands wild cards and supports multiple paths separated by +, or loads all the files of a subfolder,
    /// if the syntax for the path is 'FolderPath/...' (separator would be OS relevant).
    /// </remarks>
    public sealed class MultiFileSource : IMultiStreamSource
    {
        private readonly string[] _paths;

        /// <summary>
        /// Initializes a new instance of <see cref="MultiFileSource"/>.
        /// In case of usage from Maml, the paths would be wildcard concatenated in the first string of <paramref name="paths"/>.
        /// </summary>
        /// <param name="paths">The paths of the files to load.</param>
        /// <remarks>
        /// The provided <paramref name="paths"/> can utilize wildcards to load all source files. For example:
        /// paths = "Data/*" includes all files in directory Data
        /// paths = "DataFolder/.../*" includes all files in all subdirectories inside directory Data.
        /// paths = "Data1/*", "Data2/*" includes all files in directories Data1 and Data2
        /// </remarks>
        public MultiFileSource(params string[] paths)
        {
            Contracts.CheckValueOrNull(paths);

            // calling the ctor passing null, creates an array of 1, null element
            // The types using MFS know how to account for an empty path
            // if the paths array is empty, therefore keeping that behavior.
            if (paths == null || (paths.Length == 1 && paths[0] == null))
            {
                _paths = new string[0];
                return;
            }

            List<string> concatenated = new List<string>();
            if (paths != null)
            {
                foreach (string path in paths)
                    foreach (string rPath in StreamUtils.ExpandWildCards(path))
                        concatenated.Add(rPath);
            }
            else
                concatenated = null;

            if (concatenated != null && concatenated.Count > 0)
            {
                _paths = concatenated.ToArray();
            }
            else
                _paths = paths;
        }

        public int Count
        {
            get { return _paths.Length; }
        }

        public string GetPathOrNull(int index)
        {
            Contracts.CheckParam(0 <= index && index < Count, nameof(index));
            return _paths[index];
        }

        public Stream Open(int index)
        {
            Contracts.CheckParam(0 <= index && index < Count, nameof(index));

            var path = _paths[index];
            try
            {
                return StreamUtils.OpenInStream(path);
            }
            catch (Exception e)
            {
                throw Contracts.ExceptIO(e, "Could not open file '{0}'. Error is: {1}", path, e.Message);
            }
        }

        public TextReader OpenTextReader(int index)
        {
            return new StreamReader(Open(index));
        }
    }

    /// <summary>
    /// Wraps an <see cref="IFileHandle"/> as an IMultiStreamSource.
    /// </summary>
    public sealed class FileHandleSource : IMultiStreamSource
    {
        private readonly IFileHandle _file;

        public FileHandleSource(IFileHandle file)
        {
            Contracts.CheckValue(file, nameof(file));
            Contracts.CheckParam(file.CanRead, nameof(file), "File handle must be readable");
            _file = file;
        }

        public int Count
        {
            get { return 1; }
        }

        public string GetPathOrNull(int index)
        {
            Contracts.CheckParam(0 <= index && index < Count, nameof(index));
            return null;
        }

        public Stream Open(int index)
        {
            Contracts.CheckParam(0 <= index && index < Count, nameof(index));
            return _file.OpenReadStream();
        }

        public TextReader OpenTextReader(int index)
        {
            return new StreamReader(Open(index));
        }
    }
}
