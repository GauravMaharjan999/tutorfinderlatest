﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Microsoft.ML.CodeGenerator.CodeGenerator
{
    internal class CSharpProject : List<ICSharpFile>, ICSharpProject
    {
        public string Name { get; set; }

        /// <summary>
        /// Write Project to location
        /// </summary>
        /// <param name="location">full path of destinate directory</param>
        public void WriteToDisk(string location)
        {
            foreach (var file in this)
            {
                file.WriteToDisk(location);
            }
        }
    }
}
