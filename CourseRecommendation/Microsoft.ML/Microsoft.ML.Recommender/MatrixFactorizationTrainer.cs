﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.ML;
using Microsoft.ML.CommandLine;
using Microsoft.ML.Data;
using Microsoft.ML.EntryPoints;
using Microsoft.ML.Internal.Internallearn;
using Microsoft.ML.Recommender;
using Microsoft.ML.Recommender.Internal;
using Microsoft.ML.Runtime;
using Microsoft.ML.Trainers;
using Microsoft.ML.Trainers.Recommender;

[assembly: LoadableClass(MatrixFactorizationTrainer.Summary, typeof(MatrixFactorizationTrainer), typeof(MatrixFactorizationTrainer.Options),
    new Type[] { typeof(SignatureTrainer), typeof(SignatureMatrixRecommendingTrainer) },
    "Matrix Factorization", MatrixFactorizationTrainer.LoadNameValue, "libmf", "mf")]

namespace Microsoft.ML.Trainers
{
    /// <summary>
    /// The <see cref="IEstimator{TTransformer}"/> to predict elements in a matrix using matrix factorization (also known as a type of <a href="https://en.wikipedia.org/wiki/Collaborative_filtering">collaborative filtering</a>).
    /// </summary>
    /// <remarks>
    /// <format type="text/markdown"><![CDATA[
    /// To create this trainer, use [MatrixFactorization](xref:Microsoft.ML.RecommendationCatalog.RecommendationTrainers.MatrixFactorization(System.String,System.String,System.String,System.Int32,System.Double,System.Int32))
    /// or [MatrixFactorization(Options)](xref:Microsoft.ML.RecommendationCatalog.RecommendationTrainers.MatrixFactorization(Microsoft.ML.Trainers.MatrixFactorizationTrainer.Options)).
    ///
    /// ### Input and Output Columns
    /// There are three input columns required, one for matrix row indexes, one for matrix column indexes, and one for
    /// values (i.e., labels) in matrix.
    /// They together define a matrix in [COO](https://en.wikipedia.org/wiki/Sparse_matrix#Coordinate_list_(COO)) format.
    /// The type for label column is a vector of <xref:System.Single> while the other two columns are
    /// [key](xref:Microsoft.ML.Data.KeyDataViewType) type scalar.
    ///
    /// | Output Column Name | Column Type | Description|
    /// | -- | -- | -- |
    /// | `Score` | <xref:System.Single> | The predicted matrix value at the location specified by input columns (row index column and column index column). |
    ///
    /// ### Trainer Characteristics
    /// |  |  |
    /// | -- | -- |
    /// | Machine learning task | Recommender systems |
    /// | Is normalization required? | Yes |
    /// | Is caching required? | Yes |
    /// | Required NuGet in addition to Microsoft.ML | Microsoft.ML.Recommender |
    /// | Exportable to ONNX | No |
    ///
    /// ### Background
    /// The basic idea of matrix factorization is finding two low-rank factor matrices to approximate the training matrix.
    /// In this module, the expected training data (the factorized matrix) is a list of tuples.
    /// Every tuple consists of a column index, a row index,
    /// and the value at the location specified by the two indices. For an example data structure of a tuple, one can use:
    ///
    /// ```csharp
    /// // The following variables defines the shape of a m-by-n matrix. Indexes start with 0; that is, our indexing system
    /// // is 0-based.
    /// const int m = 60;
    /// const int n = 100;
    ///
    /// // A tuple of row index, column index, and rating. It specifies a value in the rating matrix.
    /// class MatrixElement
    /// {
    ///     // Matrix column index starts from 0 and is at most n-1.
    ///     [KeyType(n)]
    ///     public uint MatrixColumnIndex;
    ///     // Matrix row index starts from 0 and is at most m-1.
    ///     [KeyType(m)]
    ///     public uint MatrixRowIndex;
    ///     // The rating at the MatrixColumnIndex-th column and the MatrixRowIndex-th row.
    ///     public float Value;
    /// }
    /// ```
    ///
    /// Notice that it's not necessary to specify all entries in the training matrix, so matrix factorization can be used to fill <i>missing values</i>.
    /// This behavior is very helpful when building recommender systems.
    ///
    /// To provide a better understanding on practical uses of matrix factorization, let's consider music recommendation as an example.
    /// Assume that user IDs and music IDs are used as row and column indexes, respectively, and matrix's values are ratings provided by those users.
    /// That is, rating $r$ at row $u$ and column $v$ means that user $u$ give $r$ to item $v$.
    /// An incomplete matrix is very common because not all users may provide their feedbacks to all products (for example, no one can rate ten million songs).
    /// Assume that $R\in{\mathbb R}^{m\times n}$ is a m-by-n rating matrix and the [rank](https://en.wikipedia.org/wiki/Rank_(linear_algebra)) of the two factor matrices are $P\in {\mathbb R}^{k\times m}$ and $Q\in {\mathbb R}^{k\times n}$, where $k$ is the approximation rank.
    /// The predicted rating at the $u$-th row and the $v$-th column in $R$ would be the inner product of the $u$-th row of $P$ and the $v$-th row of $Q$; that is, $R$ is approximated by the product of $P$'s transpose ($P^T$) and $Q$.
    /// Note that $k$ is usually much smaller than $m$ and $n$, so $P^T Q$ is usually called a low-rank approximation of $R$.
    ///
    /// This trainer includes a [stochastic gradient method](https://en.wikipedia.org/wiki/Stochastic_gradient_descent) and a [coordinate descent method](https://en.wikipedia.org/wiki/Coordinate_descent) for finding $P$ and $Q$ via minimizing the distance between (non-missing part of) $R$ and its approximation $P^T Q$.
    /// The coordinate descent method included is specifically for one-class matrix factorization where all observed ratings are positive signals (that is, all rating values are 1).
    /// Notice that the only way to invoke one-class matrix factorization is to assign [one-class squared loss](xref:Microsoft.ML.Trainers.MatrixFactorizationTrainer.LossFunctionType.SquareLossOneClass)
    /// to [loss function](xref:Microsoft.ML.Trainers.MatrixFactorizationTrainer.Options.LossFunction)
    /// when calling [MatrixFactorization(Options)](xref:Microsoft.ML.RecommendationCatalog.RecommendationTrainers.MatrixFactorization(Microsoft.ML.Trainers.MatrixFactorizationTrainer.Options)).
    /// See Page 6 and Page 28 [here](https://www.csie.ntu.edu.tw/~cjlin/talks/facebook.pdf) for a brief introduction to standard matrix factorization and one-class matrix factorization.
    /// The [default setting](xref:Microsoft.ML.Trainers.MatrixFactorizationTrainer.LossFunctionType.SquareLossRegression) induces standard matrix factorization.
    /// The underlying library used in ML.NET matrix factorization can be found on [a Github repository](https://github.com/cjlin1/libmf).
    ///
    /// For users interested in the mathematical details, please see the references below.
    ///
    /// * For the multi-threading implementation of the used stochastic gradient method, see [A Fast Parallel Stochastic Gradient Method for Matrix Factorization in Shared Memory Systems](https://www.csie.ntu.edu.tw/~cjlin/papers/libmf/libmf_journal.pdf).
    /// * For the computation happening inside a single thread, see [A Learning-rate Schedule for Stochastic Gradient Methods to Matrix Factorization](https://www.csie.ntu.edu.tw/~cjlin/papers/libmf/mf_adaptive_pakdd.pdf).
    /// * For the parallel coordinate descent method used and one-class matrix factorization formula, see [Selection of Negative Samples for One-class Matrix Factorization](https://www.csie.ntu.edu.tw/~cjlin/papers/one-class-mf/biased-mf-sdm-with-supp.pdf).
    /// * For details in the underlying library used, see [LIBMF: A Library for Parallel Matrix Factorization in Shared-memory Systems](https://www.csie.ntu.edu.tw/~cjlin/papers/libmf/libmf_open_source.pdf).
    ///
    /// Check the See Also section for links to usage examples.
    /// ]]>
    /// </format>
    /// </remarks>
    /// <seealso cref="Microsoft.ML.RecommendationCatalog.RecommendationTrainers.MatrixFactorization(string, string, string, int, double, int)"/>
    /// <seealso cref="Microsoft.ML.RecommendationCatalog.RecommendationTrainers.MatrixFactorization(MatrixFactorizationTrainer.Options)"/>
    /// <seealso cref="Options"/>
    public sealed class MatrixFactorizationTrainer : ITrainer<MatrixFactorizationModelParameters>,
        ITrainerEstimator<MatrixFactorizationPredictionTransformer, MatrixFactorizationModelParameters>
    {
        /// <summary>
        /// Type of loss function.
        /// </summary>
        public enum LossFunctionType
        {
            /// <summary>
            /// Used in traditional collaborative filtering problem with squared loss.
            /// </summary>
            /// <remarks>
            /// See <a href="https://www.csie.ntu.edu.tw/~cjlin/papers/libmf/mf_adaptive_pakdd.pdf">Equation</a> (1).
            /// </remarks>
            SquareLossRegression = 0,

            /// <summary>
            /// Used in implicit-feedback recommendation problem.
            /// </summary>
            /// <remarks>
            /// See <a href="http://yifanhu.net/PUB/cf.pdf">Equation</a> (3).
            /// </remarks>
            SquareLossOneClass = 12
        };

        /// <summary>
        /// Options for the <see cref="MatrixFactorizationTrainer"/> as used in [MatrixFactorization(Options)](xref:Microsoft.ML.RecommendationCatalog.RecommendationTrainers.MatrixFactorization(Microsoft.ML.Trainers.MatrixFactorizationTrainer.Options)).
        /// </summary>
        public sealed class Options
        {
            /// <summary>
            /// The name of variable (i.e., Column in a <see cref="IDataView"/> type system) used as matrix's column index.
            /// The column data must be <see cref="System.Single"/>.
            /// </summary>
            public string MatrixColumnIndexColumnName;

            /// <summary>
            /// The name of variable (i.e., column in a <see cref="IDataView"/> type system) used as matrix's row index.
            /// The column data must be <see cref="Microsoft.ML.Data.KeyDataViewType"/>.
            /// </summary>
            public string MatrixRowIndexColumnName;

            /// <summary>
            /// The name variable (i.e., column in a <see cref="IDataView"/> type system) used as matrix's element value.
            /// The column data must be <see cref="Microsoft.ML.Data.KeyDataViewType"/>.
            /// </summary>
            public string LabelColumnName;

            /// <summary>
            /// Loss function minimized for finding factor matrices.
            /// </summary>
            /// <remarks>
            /// Two values are allowed, <see cref="LossFunctionType.SquareLossRegression"/> or <see cref="LossFunctionType.SquareLossOneClass"/>.
            /// The <see cref="LossFunctionType.SquareLossRegression"/> means traditional collaborative filtering problem with squared loss.
            /// The <see cref="LossFunctionType.SquareLossOneClass"/> triggers one-class matrix factorization for implicit-feedback recommendation problem.
            /// </remarks>
            [Argument(ArgumentType.AtMostOnce, HelpText = "Loss function minimized for finding factor matrices.")]
            [TGUI(SuggestedSweeps = "0,12")]
            [TlcModule.SweepableDiscreteParam("LossFunction", new object[] { LossFunctionType.SquareLossRegression, LossFunctionType.SquareLossOneClass })]
            public LossFunctionType LossFunction = Defaults.LossFunction;

            /// <summary>
            /// Regularization parameter.
            /// </summary>
            /// <remarks>
            /// It's the weight of factor matrices Frobenius norms in the objective function minimized by matrix factorization's algorithm. A small value could cause over-fitting.
            /// </remarks>
            [Argument(ArgumentType.AtMostOnce, HelpText = "Regularization parameter. " +
                "It's the weight of factor matrices Frobenius norms in the objective function minimized by matrix factorization's algorithm. " +
                "A small value could cause over-fitting.")]
            [TGUI(SuggestedSweeps = "0.01,0.05,0.1,0.5,1")]
            [TlcModule.SweepableDiscreteParam("Lambda", new object[] { 0.01f, 0.05f, 0.1f, 0.5f, 1f })]
            public double Lambda = Defaults.Lambda;

            /// <summary>
            /// Rank of approximation matrices.
            /// </summary>
            /// <remarks>
            /// If input data has size of m-by-n we would build two approximation matrices m-by-k and k-by-n where k is approximation rank.
            /// </remarks>
            [Argument(ArgumentType.AtMostOnce, HelpText = "Latent space dimension (denoted by k). If the factorized matrix is m-by-n, " +
                "two factor matrices found by matrix factorization are m-by-k and k-by-n, respectively. " +
                "This value is also known as the rank of matrix factorization because k is generally much smaller than m and n.", ShortName = "K")]
            [TGUI(SuggestedSweeps = "8,16,64,128")]
            [TlcModule.SweepableDiscreteParam("K", new object[] { 8, 16, 64, 128 })]
            public int ApproximationRank = Defaults.ApproximationRank;

            /// <summary>
            /// Number of training iterations.
            /// </summary>
            [Argument(ArgumentType.AtMostOnce, HelpText = "Training iterations; that is, the times that the training algorithm iterates through the whole training data once.", ShortName = "iter,numiterations")]
            [TGUI(SuggestedSweeps = "10,20,40")]
            [TlcModule.SweepableDiscreteParam("NumIterations", new object[] { 10, 20, 40 })]
            public int NumberOfIterations = Defaults.NumIterations;

            ///<summary>
            /// Initial learning rate. It specifies the speed of the training algorithm.
            ///</summary>
            ///<remarks>
            /// Small value may increase the number of iterations needed to achieve a reasonable result.
            /// Large value may lead to numerical difficulty such as a infinity value.
            ///</remarks>
            [Argument(ArgumentType.AtMostOnce, HelpText = "Initial learning rate. It specifies the speed of the training algorithm. " +
                "Small value may increase the number of iterations needed to achieve a reasonable result. Large value may lead to numerical difficulty such as a infinity value.", ShortName = "Eta")]
            [TGUI(SuggestedSweeps = "0.001,0.01,0.1")]
            [TlcModule.SweepableDiscreteParam("Eta", new object[] { 0.001f, 0.01f, 0.1f })]
            public double LearningRate = Defaults.LearningRate;

            /// <summary>
            /// Importance of unobserved entries' loss in one-class matrix factorization. Applicable if <see cref="LossFunction"/> set to <see cref="LossFunctionType.SquareLossOneClass"/>
            /// </summary>
            /// <remarks>
            /// Importance of unobserved (i.e., negative) entries' loss in one-class matrix factorization.
            /// In general, only a few of matrix entries (e.g., less than 1%) in the training are observed (i.e., positive).
            /// To balance the contributions from unobserved and observed in the overall loss function, this parameter is
            /// usually a small value so that the solver is able to find a factorization equally good to unobserved and observed
            /// entries. If only 10000 observed entries present in a 200000-by-300000 training matrix, one can try Alpha = 10000 / (200000*300000 - 10000).
            /// When most entries in the training matrix are observed, one can use Alpha >> 1; for example, if only 10000 in previous
            /// matrix is not observed, one can try Alpha = (200000 * 300000 - 10000) / 10000. Consequently,
            /// Alpha = (# of observed entries) / (# of unobserved entries) can make observed and unobserved entries equally important
            /// in the minimized loss function. However, the best setting in machine learning is always data-dependent so user still needs to
            /// try multiple values.
            /// </remarks>
            [Argument(ArgumentType.AtMostOnce, HelpText = "Importance of unobserved entries' loss in one-class matrix factorization.")]
            [TGUI(SuggestedSweeps = "1,0.01,0.0001,0.000001")]
            [TlcModule.SweepableDiscreteParam("Alpha", new object[] { 1f, 0.01f, 0.0001f, 0.000001f })]
            public double Alpha = Defaults.Alpha;

            /// <summary>
            /// Desired negative entries value in one-class matrix factorization. Applicable if <see cref="LossFunction"/> set to <see cref="LossFunctionType.SquareLossOneClass"/>
            /// </summary>
            /// <remarks>
            /// In one-class matrix factorization, all matrix values observed are one (which can be viewed as positive cases in binary classification)
            /// while unobserved values (which can be viewed as negative cases in binary classification) need to be specified manually using this option.
            /// </remarks>
            [Argument(ArgumentType.AtMostOnce, HelpText = "Desired negative entries' value in one-class matrix factorization")]
            [TGUI(SuggestedSweeps = "0.000001,0,0001,0.01")]
            [TlcModule.SweepableDiscreteParam("C", new object[] { 0.000001f, 0.0001f, 0.01f })]
            public double C = Defaults.C;

            /// <summary>
            /// Number of threads will be used during training. If unspecified all available threads will be use.
            /// </summary>
            [Argument(ArgumentType.AtMostOnce, HelpText = "Number of threads can be used in the training procedure.", ShortName = "t,numthreads")]
            public int? NumberOfThreads;

            /// <summary>
            /// Suppress writing additional information to output.
            /// </summary>
            [Argument(ArgumentType.AtMostOnce, HelpText = "Suppress writing additional information to output.")]
            public bool Quiet = Defaults.Quiet;

            /// <summary>
            /// Force the factor matrices to be non-negative.
            /// </summary>
            [Argument(ArgumentType.AtMostOnce, HelpText = "Force the factor matrices to be non-negative.", ShortName = "nn")]
            public bool NonNegative = Defaults.NonNegative;
        };

        [BestFriend]
        internal static class Defaults
        {
            public const bool Quiet = false;
            public const bool NonNegative = false;
            public const double C = 0.000001f;
            public const double Alpha = 0.0001f;
            public const double LearningRate = 0.1;
            public const int NumIterations = 20;
            public const int ApproximationRank = 8;
            public const double Lambda = 0.1;
            public const LossFunctionType LossFunction = LossFunctionType.SquareLossRegression;
        }

        internal const string Summary = "From pairs of row/column indices and a value of a matrix, this trains a predictor capable of filling in unknown entries of the matrix, "
            + "using a low-rank matrix factorization. This technique is often used in recommender system, where the row and column indices indicate users and items, "
            + "and the values of the matrix are ratings. ";

        private readonly IHost _host;

        // LIBMF's parameter
        private readonly int _fun;
        private readonly double _lambda;
        private readonly int _k;
        private readonly int _iter;
        private readonly double _eta;
        private readonly double _alpha;
        private readonly double _c;
        private readonly int _threads;
        private readonly bool _quiet;
        private readonly bool _doNmf;

        PredictionKind ITrainer.PredictionKind => PredictionKind.Recommendation;

        /// <summary>
        /// The <see cref="TrainerInfo"/> contains general parameters for this trainer.
        /// </summary>
        public TrainerInfo Info => _info;

        internal const string LoadNameValue = "MatrixFactorization";

        /// <summary>
        /// The row index, column index, and label columns needed to specify the training matrix. This trainer uses tuples of (row index, column index, label value) to specify a matrix.
        /// For example, a 2-by-2 matrix
        ///   [9, 4]
        ///   [8, 7]
        /// can be encoded as tuples (0, 0, 9), (0, 1, 4), (1, 0, 8), and (1, 1, 7). It means that the row/column/label column contains [0, 0, 1, 1]/
        /// [0, 1, 0, 1]/[9, 4, 8, 7].
        /// </summary>

        /// <summary>
        /// The name of variable (i.e., Column in a <see cref="IDataView"/> type system) used be as matrix's column index.
        /// </summary>
        internal readonly string MatrixColumnIndexName;

        /// <summary>
        /// The name of variable (i.e., column in a <see cref="IDataView"/> type system) used as matrix's row index.
        /// </summary>
        internal readonly string MatrixRowIndexName;

        /// <summary>
        /// The name variable (i.e., column in a <see cref="IDataView"/> type system) used as matrix's element value.
        /// </summary>
        internal readonly string LabelName;

        private readonly TrainerInfo _info;

        /// <summary>
        /// Initializes a new instance of <see cref="MatrixFactorizationTrainer"/> through the <see cref="Options"/> class.
        /// </summary>
        /// <param name="env">The private instance of <see cref="IHostEnvironment"/>.</param>
        /// <param name="options">An instance of <see cref="Options"/> to apply advanced parameters to the algorithm.</param>
        [BestFriend]
        internal MatrixFactorizationTrainer(IHostEnvironment env, Options options)
        {
            Contracts.CheckValue(env, nameof(env));
            _host = env.Register(LoadNameValue);

            const string posError = "Parameter must be positive";
            _host.CheckValue(options, nameof(options));
            _host.CheckUserArg(options.ApproximationRank > 0, nameof(options.ApproximationRank), posError);
            _host.CheckUserArg(!options.NumberOfThreads.HasValue || options.NumberOfThreads > 0, nameof(options.NumberOfThreads), posError);
            _host.CheckUserArg(options.NumberOfIterations > 0, nameof(options.NumberOfIterations), posError);
            _host.CheckUserArg(options.Lambda > 0, nameof(options.Lambda), posError);
            _host.CheckUserArg(options.LearningRate > 0, nameof(options.LearningRate), posError);
            _host.CheckUserArg(options.Alpha > 0, nameof(options.Alpha), posError);

            _fun = (int)options.LossFunction;
            _lambda = options.Lambda;
            _k = options.ApproximationRank;
            _iter = options.NumberOfIterations;
            _eta = options.LearningRate;
            _alpha = options.Alpha;
            _c = options.C;
            _threads = options.NumberOfThreads ?? Environment.ProcessorCount;
            _quiet = options.Quiet;
            _doNmf = options.NonNegative;

            _info = new TrainerInfo(normalization: false, caching: false);

            LabelName = options.LabelColumnName;
            MatrixColumnIndexName = options.MatrixColumnIndexColumnName;
            MatrixRowIndexName = options.MatrixRowIndexColumnName;
        }

        /// <summary>
        /// Initializes a new instance of <see cref="MatrixFactorizationTrainer"/>.
        /// </summary>
        /// <param name="env">The private instance of <see cref="IHostEnvironment"/>.</param>
        /// <param name="labelColumnName">The name of the label column.</param>
        /// <param name="matrixColumnIndexColumnName">The name of the column hosting the matrix's column IDs.</param>
        /// <param name="matrixRowIndexColumnName">The name of the column hosting the matrix's row IDs.</param>
        /// <param name="approximationRank">Rank of approximation matrices.</param>
        /// <param name="learningRate">Initial learning rate. It specifies the speed of the training algorithm.</param>
        /// <param name="numIterations">Number of training iterations.</param>
        [BestFriend]
        internal MatrixFactorizationTrainer(IHostEnvironment env,
            string labelColumnName,
            string matrixColumnIndexColumnName,
            string matrixRowIndexColumnName,
            int approximationRank = Defaults.ApproximationRank,
            double learningRate = Defaults.LearningRate,
            int numIterations = Defaults.NumIterations)
        {
            Contracts.CheckValue(env, nameof(env));
            _host = env.Register(LoadNameValue);

            var args = new Options();

            _fun = (int)args.LossFunction;
            _k = approximationRank;
            _iter = numIterations;
            _eta = learningRate;
            _alpha = args.Alpha;
            _lambda = args.Lambda;
            _c = args.C;
            _threads = args.NumberOfThreads ?? Environment.ProcessorCount;
            _quiet = args.Quiet;
            _doNmf = args.NonNegative;

            _info = new TrainerInfo(normalization: false, caching: false);

            LabelName = labelColumnName;
            MatrixColumnIndexName = matrixColumnIndexColumnName;
            MatrixRowIndexName = matrixRowIndexColumnName;
        }

        /// <summary>
        /// Train a matrix factorization model based on training data, validation data, and so on in the given context.
        /// </summary>
        /// <param name="context">The information collection needed for training. <see cref="TrainContext"/> for details.</param>
        private MatrixFactorizationModelParameters Train(TrainContext context)
        {
            _host.CheckValue(context, nameof(context));
            using (var ch = _host.Start("Training"))
            {
                return TrainCore(ch, context.TrainingSet, context.ValidationSet);
            }
        }

        IPredictor ITrainer.Train(TrainContext context) => Train(context);
        MatrixFactorizationModelParameters ITrainer<MatrixFactorizationModelParameters>.Train(TrainContext context) => Train(context);

        private MatrixFactorizationModelParameters TrainCore(IChannel ch, RoleMappedData data, RoleMappedData validData = null)
        {
            _host.AssertValue(ch);
            ch.AssertValue(data);
            ch.AssertValueOrNull(validData);

            ch.CheckParam(data.Schema.Label.HasValue, nameof(data), "Input data did not have a unique label");
            RecommenderUtils.CheckAndGetMatrixIndexColumns(data, out var matrixColumnIndexColInfo, out var matrixRowIndexColInfo, isDecode: false);
            var labelCol = data.Schema.Label.Value;
            if (labelCol.Type != NumberDataViewType.Single && labelCol.Type != NumberDataViewType.Double)
                throw ch.Except("Column '{0}' for label should be floating point, but is instead {1}", labelCol.Name, labelCol.Type);
            MatrixFactorizationModelParameters predictor;
            if (validData != null)
            {
                ch.CheckValue(validData, nameof(validData));
                ch.CheckParam(validData.Schema.Label.HasValue, nameof(validData), "Input validation data did not have a unique label");
                RecommenderUtils.CheckAndGetMatrixIndexColumns(validData, out var validMatrixColumnIndexColInfo, out var validMatrixRowIndexColInfo, isDecode: false);
                var validLabelCol = validData.Schema.Label.Value;
                if (validLabelCol.Type != NumberDataViewType.Single && validLabelCol.Type != NumberDataViewType.Double)
                    throw ch.Except("Column '{0}' for validation label should be floating point, but is instead {1}", validLabelCol.Name, validLabelCol.Type);

                if (!matrixColumnIndexColInfo.Type.Equals(validMatrixColumnIndexColInfo.Type))
                {
                    throw ch.ExceptParam(nameof(validData), "Train and validation sets' matrix-column types differed, {0} vs. {1}",
                        matrixColumnIndexColInfo.Type, validMatrixColumnIndexColInfo.Type);
                }
                if (!matrixRowIndexColInfo.Type.Equals(validMatrixRowIndexColInfo.Type))
                {
                    throw ch.ExceptParam(nameof(validData), "Train and validation sets' matrix-row types differed, {0} vs. {1}",
                        matrixRowIndexColInfo.Type, validMatrixRowIndexColInfo.Type);
                }
            }

            int colCount = matrixColumnIndexColInfo.Type.GetKeyCountAsInt32(_host);
            int rowCount = matrixRowIndexColInfo.Type.GetKeyCountAsInt32(_host);
            ch.Assert(rowCount > 0);
            ch.Assert(colCount > 0);

            // Checks for equality on the validation set ensure it is correct here.
            using (var cursor = data.Data.GetRowCursor(matrixColumnIndexColInfo, matrixRowIndexColInfo, data.Schema.Label.Value))
            {
                // LibMF works only over single precision floats, but we want to be able to consume either.
                var labGetter = RowCursorUtils.GetGetterAs<float>(NumberDataViewType.Single, cursor, data.Schema.Label.Value.Index);
                var matrixColumnIndexGetter = RowCursorUtils.GetGetterAs<uint>(NumberDataViewType.UInt32, cursor, matrixColumnIndexColInfo.Index);
                var matrixRowIndexGetter = RowCursorUtils.GetGetterAs<uint>(NumberDataViewType.UInt32, cursor, matrixRowIndexColInfo.Index);

                if (validData == null)
                {
                    // Have the trainer do its work.
                    using (var buffer = PrepareBuffer())
                    {
                        buffer.Train(ch, rowCount, colCount, cursor, labGetter, matrixRowIndexGetter, matrixColumnIndexGetter);
                        predictor = new MatrixFactorizationModelParameters(_host, buffer, (KeyDataViewType)matrixColumnIndexColInfo.Type, (KeyDataViewType)matrixRowIndexColInfo.Type);
                    }
                }
                else
                {
                    RecommenderUtils.CheckAndGetMatrixIndexColumns(validData, out var validMatrixColumnIndexColInfo, out var validMatrixRowIndexColInfo, isDecode: false);
                    using (var validCursor = validData.Data.GetRowCursor(matrixColumnIndexColInfo, matrixRowIndexColInfo, data.Schema.Label.Value))
                    {
                        ValueGetter<float> validLabelGetter = RowCursorUtils.GetGetterAs<float>(NumberDataViewType.Single, validCursor, validData.Schema.Label.Value.Index);
                        var validMatrixColumnIndexGetter = RowCursorUtils.GetGetterAs<uint>(NumberDataViewType.UInt32, validCursor, validMatrixColumnIndexColInfo.Index);
                        var validMatrixRowIndexGetter = RowCursorUtils.GetGetterAs<uint>(NumberDataViewType.UInt32, validCursor, validMatrixRowIndexColInfo.Index);

                        // Have the trainer do its work.
                        using (var buffer = PrepareBuffer())
                        {
                            buffer.TrainWithValidation(ch, rowCount, colCount,
                                cursor, labGetter, matrixRowIndexGetter, matrixColumnIndexGetter,
                                validCursor, validLabelGetter, validMatrixRowIndexGetter, validMatrixColumnIndexGetter);
                            predictor = new MatrixFactorizationModelParameters(_host, buffer, (KeyDataViewType)matrixColumnIndexColInfo.Type, (KeyDataViewType)matrixRowIndexColInfo.Type);
                        }
                    }
                }

            }
            return predictor;
        }

        private SafeTrainingAndModelBuffer PrepareBuffer()
        {
            return new SafeTrainingAndModelBuffer(_host, _fun, _k, _threads, 2 * _threads + 1,
                _iter, _lambda, _eta, _alpha, _c, _doNmf, _quiet, copyData: false);
        }

        /// <summary>
        /// Trains a <see cref="MatrixFactorizationTrainer"/> using both training and validation data, returns a <see cref="MatrixFactorizationPredictionTransformer"/>.
        /// </summary>
        /// <param name="trainData">The training data set.</param>
        /// <param name="validationData">The validation data set.</param>
        public MatrixFactorizationPredictionTransformer Fit(IDataView trainData, IDataView validationData)
        {
            MatrixFactorizationModelParameters model = null;

            var roles = new List<KeyValuePair<RoleMappedSchema.ColumnRole, string>>();
            roles.Add(new KeyValuePair<RoleMappedSchema.ColumnRole, string>(RoleMappedSchema.ColumnRole.Label, LabelName));
            roles.Add(new KeyValuePair<RoleMappedSchema.ColumnRole, string>(RecommenderUtils.MatrixColumnIndexKind.Value, MatrixColumnIndexName));
            roles.Add(new KeyValuePair<RoleMappedSchema.ColumnRole, string>(RecommenderUtils.MatrixRowIndexKind.Value, MatrixRowIndexName));

            var trainingData = new RoleMappedData(trainData, roles);
            var validData = validationData == null ? null : new RoleMappedData(validationData, roles);
            using (var ch = _host.Start("Training"))
            {
                model = TrainCore(ch, trainingData, validData);
            }

            return new MatrixFactorizationPredictionTransformer(_host, model, trainData.Schema, MatrixColumnIndexName, MatrixRowIndexName);
        }

        /// <summary>
        /// <summary> Trains and returns a <see cref="MatrixFactorizationPredictionTransformer"/>.</summary>
        /// </summary>
        /// <param name="input">The training data set.</param>
        public MatrixFactorizationPredictionTransformer Fit(IDataView input) => Fit(input, null);

        /// <summary>
        /// Schema propagation for transformers. Returns the output schema of the data, if
        /// the input schema is like the one provided.
        /// </summary>
        public SchemaShape GetOutputSchema(SchemaShape inputSchema)
        {
            _host.CheckValue(inputSchema, nameof(inputSchema));

            void CheckColumnsCompatible(SchemaShape.Column cachedColumn, string columnRole)
            {
                if (!inputSchema.TryFindColumn(cachedColumn.Name, out var col))
                    throw _host.ExceptSchemaMismatch(nameof(col), columnRole, cachedColumn.Name);

                if (!cachedColumn.IsCompatibleWith(col))
                    throw _host.ExceptSchemaMismatch(nameof(inputSchema), columnRole, cachedColumn.Name,
                        cachedColumn.GetTypeString(), col.GetTypeString());
            }

            // Check if label column is good.
            var labelColumn = new SchemaShape.Column(LabelName, SchemaShape.Column.VectorKind.Scalar, NumberDataViewType.Single, false);
            CheckColumnsCompatible(labelColumn, "label");

            // Check if columns of matrix's row and column indexes are good. Note that column of IDataView and column of matrix are two different things.
            var matrixColumnIndexColumn = new SchemaShape.Column(MatrixColumnIndexName, SchemaShape.Column.VectorKind.Scalar, NumberDataViewType.UInt32, true);
            var matrixRowIndexColumn = new SchemaShape.Column(MatrixRowIndexName, SchemaShape.Column.VectorKind.Scalar, NumberDataViewType.UInt32, true);
            CheckColumnsCompatible(matrixColumnIndexColumn, "matrixColumnIndex");
            CheckColumnsCompatible(matrixRowIndexColumn, "matrixRowIndex");

            // Input columns just pass through so that output column dictionary contains all input columns.
            var outColumns = inputSchema.ToDictionary(x => x.Name);

            // Add columns produced by this estimator.
            foreach (var col in GetOutputColumnsCore(inputSchema))
                outColumns[col.Name] = col;

            return new SchemaShape(outColumns.Values);
        }

        private SchemaShape.Column[] GetOutputColumnsCore(SchemaShape inputSchema)
        {
            bool success = inputSchema.TryFindColumn(LabelName, out var labelCol);
            Contracts.Assert(success);

            return new[]
            {
                new SchemaShape.Column(DefaultColumnNames.Score, SchemaShape.Column.VectorKind.Scalar, NumberDataViewType.Single, false, new SchemaShape(AnnotationUtils.GetTrainerOutputAnnotation())),
            };
        }
    }
}
