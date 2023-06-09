﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Microsoft.ML.Data;

namespace Microsoft.ML
{
    /// <summary>
    /// Class used to load data from one or more files.
    /// </summary>
    public static class DataLoaderExtensions
    {
        /// <summary>
        /// Loads data from one or more file <paramref name="path"/> into an <see cref="IDataView"/>.
        /// Note that <see cref="IDataView"/>'s are lazy, so no actual loading happens here, just schema validation.
        /// </summary>
        /// <param name="loader">The loader to use.</param>
        /// <param name="path">One or more paths from which to load data.</param>
        /// <example>
        /// <format type="text/markdown">
        /// <![CDATA[
        /// [!code-csharp[Load](~/../docs/samples/docs/samples/Microsoft.ML.Samples/Dynamic/DataOperations/LoadingText.cs)]
        /// ]]>
        /// </format>
        /// </example>
        public static IDataView Load(this IDataLoader<IMultiStreamSource> loader, params string[] path)
            => loader.Load(new MultiFileSource(path));
    }
}
