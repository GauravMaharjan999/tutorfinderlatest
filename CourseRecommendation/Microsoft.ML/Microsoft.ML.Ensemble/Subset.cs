﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Collections;
using Microsoft.ML.Data;
using Microsoft.ML.Runtime;

namespace Microsoft.ML.Trainers.Ensemble
{
    internal sealed class Subset
    {
        public readonly RoleMappedData Data;
        public readonly BitArray SelectedFeatures;

        public Subset(RoleMappedData data, BitArray features = null)
        {
            Contracts.AssertValue(data);
            Data = data;
            SelectedFeatures = features;
        }
    }
}
