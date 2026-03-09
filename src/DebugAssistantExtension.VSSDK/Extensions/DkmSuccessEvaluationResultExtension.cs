
//using Microsoft.VisualStudio.Debugger;
//using Microsoft.VisualStudio.Debugger.Evaluation;

//namespace DebugAssistantExtension.Extensions;

//internal static class DkmSuccessEvaluationResultExtension
//{
//    //public static Task<DkmEvaluateExpressionAsyncResult> EvaluateExpressionAsync(
//    //    this DkmSuccessEvaluationResult evaluationResult,
//    //    DkmEvaluationFlags compilationFlags,
//    //    string text,
//    //    DkmDataItem? dataItem,
//    //    CancellationToken cancellationToken)
//    //{
//    //    return EvaluateExpressionAsync(
//    //        evaluationResult,
//    //        evaluationResult.Language,
//    //        compilationFlags,
//    //        text,
//    //        dataItem,
//    //        cancellationToken);
//    //}

//    public static async Task<DkmEvaluateExpressionAsyncResult> EvaluateExpressionAsync(
//        this DkmSuccessEvaluationResult evaluationResult,
//        DkmLanguage language,
//        DkmEvaluationFlags compilationFlags,
//        string text,
//        DkmDataItem? dataItem,
//        CancellationToken cancellationToken)
//    {
//        cancellationToken.ThrowIfCancellationRequested();
//        //ThreadHelper.ThrowIfOnUIThread();

//        var stackFrame = evaluationResult.StackFrame;
//        var expr = DkmLanguageExpression.Create(
//            language,
//            compilationFlags,
//            text,
//            dataItem);
//        var workList = DkmWorkList.Create(null);
//        var inspectionContext = evaluationResult.InspectionContext;

//        var completionSource = new TaskCompletionSource<DkmEvaluateExpressionAsyncResult>();
//        inspectionContext.EvaluateExpression(
//            workList,
//            expr,
//            stackFrame,
//            (result) =>
//            {
//                completionSource.TrySetResult(result);
//            });
//        workList.Execute();

//        using var cancellationTokenRegistration = cancellationToken.Register(() =>
//        {
//            workList.Cancel();
//            completionSource.TrySetCanceled();
//        });

//        var result = await completionSource.Task;
//        return result;
//    }
//}
