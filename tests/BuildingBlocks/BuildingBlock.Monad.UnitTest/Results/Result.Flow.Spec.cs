using System.Reflection;
using System.Runtime.CompilerServices;
using BuildingBlock.Monad.Errors;
using BuildingBlock.Monad.Results;

namespace BuildingBlock.Monad.UnitTest.Results;

/// <summary>Tests for the synchronous flow extensions in <see cref="ResultExtension"/>.</summary>
[Trait("Category", "Unit")]
public class ResultFlowSpec
{
    #region Pattern Matching

    /// <summary>Match should select the callback by state and default when the callback is missing.</summary>
    [Fact]
    public void Match_Should_Select_Callback_By_State_And_Default_When_Missing()
    {
        // Arrange
        var success = Result.Ok();
        var failure = Result.Fail(ResultStub.NotFoundError());

        // Act & Assert
        success.Match(() => "ok", _ => "fail").ShouldBe("ok");
        failure.Match(() => "ok", errors => errors[0].Code).ShouldBe("test.not_found");
        success.Match<int?>(null, _ => 1).ShouldBeNull();
        failure.Match<int?>(() => 1, null).ShouldBeNull();
    }

    /// <summary>Fold should behave like Match.</summary>
    [Fact]
    public void Fold_Should_Behave_Like_Match()
    {
        // Arrange
        var success = Result.Ok();
        var failure = Result.Fail(ResultStub.NotFoundError());

        // Act & Assert
        success.Fold(() => 1, _ => 2).ShouldBe(1);
        failure.Fold(() => 1, _ => 2).ShouldBe(2);
    }

    #endregion

    #region Map

    /// <summary>Non-generic Map should propagate failure, report missing callbacks and map success.</summary>
    [Fact]
    public void Map_Should_Propagate_Failure_Report_Missing_Callback_And_Map_Success()
    {
        // Arrange
        var failure = Result.Fail(ResultStub.NotFoundError())
            .WithMetadata("source", "kept");
        var success = Result.Ok()
            .WithMetadata("source", "kept");

        // Act & Assert
        var propagated = failure.Map(() => 42);
        propagated.IsFailure.ShouldBeTrue();
        propagated.StatusCode.ShouldBe(ResultConstant.StatusCode.NotFound);
        propagated.Metadata["source"].ShouldBe("kept");

        var missing = success.Map((Func<int>?)null);
        missing.IsFailure.ShouldBeTrue();
        missing.Errors[0].Code.ShouldBe("result.flow.callback_missing");

        var mapped = success.Map(() => 42);
        mapped.IsSuccess.ShouldBeTrue();
        mapped.Value.ShouldBe(42);
        mapped.Metadata["source"].ShouldBe("kept");
    }

    /// <summary>Generic Map should propagate failure, report missing callbacks and map success.</summary>
    [Fact]
    public void Generic_Map_Should_Propagate_Failure_Report_Missing_Callback_And_Map_Value()
    {
        // Arrange
        var failure = Result<int>.Fail(ResultStub.NotFoundError())
            .WithMetadata("source", "kept");
        var success = Result<int>.Ok(7)
            .WithMetadata("source", "kept");

        // Act & Assert
        var propagated = failure.Map(v => v + 1);
        propagated.IsFailure.ShouldBeTrue();
        propagated.StatusCode.ShouldBe(ResultConstant.StatusCode.NotFound);
        propagated.Metadata["source"].ShouldBe("kept");

        var missing = success.Map((Func<int, int>?)null);
        missing.IsFailure.ShouldBeTrue();
        missing.Errors[0].Code.ShouldBe("result.flow.callback_missing");

        var mapped = success.Map(v => v + 1);
        mapped.IsSuccess.ShouldBeTrue();
        mapped.Value.ShouldBe(8);
        mapped.Metadata["source"].ShouldBe("kept");
    }

    #endregion

    #region MapError

    /// <summary>Non-generic MapError should pass through success and map each failure error.</summary>
    [Fact]
    public void MapError_Should_Pass_Success_Map_Failures_And_Report_Missing_Callback_Or_Error()
    {
        // Arrange
        var success = Result.Ok();
        var failure = Result.Fail(ResultStub.NotFoundError())
            .WithMetadata("source", "kept");

        // Act & Assert
        failure.MapError(null).Errors[0].Code.ShouldBe("result.flow.callback_missing");

        success.MapError(null).ShouldBeSameAs(success);

        var mapped = failure.MapError(
            error => Error.Custom(error.Code, error.Message));
        mapped.IsFailure.ShouldBeTrue();
        mapped.Errors[0].Code.ShouldBe("test.not_found");
        mapped.Metadata["source"].ShouldBe("kept");

        var nullMapped = failure.MapError(_ => null!);
        nullMapped.Errors[0].Code.ShouldBe("result.flow.error_missing");
    }

    /// <summary>Generic MapError should pass through success and map each failure error.</summary>
    [Fact]
    public void Generic_MapError_Should_Pass_Success_Map_Failures_And_Report_Missing_Callback_Or_Error()
    {
        // Arrange
        var success = Result<int>.Ok(3);
        var failure = Result<int>.Fail(ResultStub.NotFoundError())
            .WithMetadata("source", "kept");

        // Act & Assert
        failure.MapError(null).Errors[0].Code.ShouldBe("result.flow.callback_missing");

        success.MapError(null).ShouldBeSameAs(success);

        var mapped = failure.MapError(
            error => Error.Custom(error.Code, error.Message));
        mapped.IsFailure.ShouldBeTrue();
        mapped.Errors[0].Code.ShouldBe("test.not_found");
        mapped.Metadata["source"].ShouldBe("kept");

        var nullMapped = failure.MapError(_ => null!);
        nullMapped.Errors[0].Code.ShouldBe("result.flow.error_missing");
    }

    #endregion

    #region Bind

    /// <summary>Non-generic Bind should propagate failure, report missing callbacks and bind success.</summary>
    [Fact]
    public void Bind_Should_Propagate_Failure_Report_Missing_And_Bind_Success()
    {
        // Arrange
        var failure = Result.Fail(ResultStub.NotFoundError())
            .WithMetadata("source", "kept");
        var success = Result.Ok()
            .WithMetadata("source", "kept");

        // Act & Assert
        var propagated = failure.Bind(() => Result<int>.Ok(1));
        propagated.IsFailure.ShouldBeTrue();
        propagated.StatusCode.ShouldBe(ResultConstant.StatusCode.NotFound);
        propagated.Metadata["source"].ShouldBe("kept");

        var missing = success.Bind((Func<Result<int>>?)null);
        missing.Errors[0].Code.ShouldBe("result.flow.callback_missing");

        var nullBound = success.Bind<int>(() => null!);
        nullBound.Errors[0].Code.ShouldBe("result.flow.result_missing");

        var bound = success.Bind(() => Result<int>.Ok(9));
        bound.IsSuccess.ShouldBeTrue();
        bound.Value.ShouldBe(9);
        bound.Metadata["source"].ShouldBe("kept");
    }

    /// <summary>Generic Bind should propagate failure, report missing callbacks and bind success.</summary>
    [Fact]
    public void Generic_Bind_Should_Propagate_Failure_Report_Missing_And_Bind_Success()
    {
        // Arrange
        var failure = Result<int>.Fail(ResultStub.NotFoundError())
            .WithMetadata("source", "kept");
        var success = Result<int>.Ok(4)
            .WithMetadata("source", "kept");

        // Act & Assert
        var propagated = failure.Bind(v => Result<string>.Ok(v.ToString()));
        propagated.IsFailure.ShouldBeTrue();
        propagated.StatusCode.ShouldBe(ResultConstant.StatusCode.NotFound);
        propagated.Metadata["source"].ShouldBe("kept");

        var missing = success.Bind((Func<int, Result<string>>?)null);
        missing.Errors[0].Code.ShouldBe("result.flow.callback_missing");

        var nullBound = success.Bind<int, string>(_ => null!);
        nullBound.Errors[0].Code.ShouldBe("result.flow.result_missing");

        var bound = success.Bind(v => Result<string>.Ok($"value-{v}"));
        bound.IsSuccess.ShouldBeTrue();
        bound.Value.ShouldBe("value-4");
        bound.Metadata["source"].ShouldBe("kept");
    }

    #endregion

    #region Flatten

    /// <summary>Flatten should unwrap success and propagate failure.</summary>
    [Fact]
    public void Flatten_Should_Unwrap_Success_And_Propagate_Failure()
    {
        // Arrange
        var failure = Result<Result<int>>.Fail(ResultStub.NotFoundError())
            .WithMetadata("source", "kept");
        var success = Result<Result<int>>.Ok(Result<int>.Ok(5))
            .WithMetadata("source", "kept");

        // Act & Assert
        var propagated = failure.Flatten();
        propagated.IsFailure.ShouldBeTrue();
        propagated.StatusCode.ShouldBe(ResultConstant.StatusCode.NotFound);
        propagated.Metadata["source"].ShouldBe("kept");

        var flattened = success.Flatten();
        flattened.IsSuccess.ShouldBeTrue();
        flattened.Value.ShouldBe(5);
        flattened.Metadata["source"].ShouldBe("kept");
    }

    /// <summary>Flatten should reject a success holder with a null inner result.</summary>
    [Fact]
    public void Flatten_With_Null_Inner_Result_Should_Fail_Result_Missing()
    {
        // Arrange — bypass factory validation to simulate a corrupted holder
        var holder = (Result<Result<int>>)RuntimeHelpers.GetUninitializedObject(
            typeof(Result<Result<int>>));
        typeof(Result)
            .GetField(
                "<IsSuccess>k__BackingField",
                BindingFlags.Instance | BindingFlags.NonPublic)!
            .SetValue(holder, true);
        typeof(Result<Result<int>>)
            .GetField(
                "<Value>k__BackingField",
                BindingFlags.Instance | BindingFlags.NonPublic)!
            .SetValue(holder, null);

        // Act
        var flattened = holder.Flatten();

        // Assert
        flattened.IsFailure.ShouldBeTrue();
        flattened.Errors[0].Code.ShouldBe("result.flow.result_missing");
    }

    #endregion

    #region Ensure

    /// <summary>Non-generic Ensure should pass failures and enforce the predicate on success.</summary>
    [Fact]
    public void Ensure_Should_Pass_Failures_And_Enforce_Predicate()
    {
        // Arrange
        var failure = Result.Fail(ResultStub.NotFoundError());
        var success = Result.Ok()
            .WithMetadata("source", "kept");
        var guard = ResultStub.ConflictError();

        // Act & Assert
        failure.Ensure(null, null).ShouldBeSameAs(failure);

        success.Ensure(null, guard).Errors[0].Code.ShouldBe("result.flow.callback_missing");
        success.Ensure(() => true, null).Errors[0].Code.ShouldBe("result.flow.error_missing");

        success.Ensure(() => true, guard).ShouldBeSameAs(success);

        var rejected = success.Ensure(() => false, guard);
        rejected.IsFailure.ShouldBeTrue();
        rejected.Errors[0].Code.ShouldBe("test.conflict");
        rejected.Metadata["source"].ShouldBe("kept");
    }

    /// <summary>Generic Ensure should pass failures and enforce the predicate on success.</summary>
    [Fact]
    public void Generic_Ensure_Should_Pass_Failures_And_Enforce_Predicate()
    {
        // Arrange
        var failure = Result<int>.Fail(ResultStub.NotFoundError());
        var success = Result<int>.Ok(7)
            .WithMetadata("source", "kept");
        var guard = ResultStub.ConflictError();

        // Act & Assert
        failure.Ensure(null, null).ShouldBeSameAs(failure);

        success.Ensure(null, guard).Errors[0].Code.ShouldBe("result.flow.callback_missing");
        success.Ensure(_ => true, null).Errors[0].Code.ShouldBe("result.flow.error_missing");

        success.Ensure(v => v > 0, guard).ShouldBeSameAs(success);

        var rejected = success.Ensure(v => v < 0, guard);
        rejected.IsFailure.ShouldBeTrue();
        rejected.Errors[0].Code.ShouldBe("test.conflict");
        rejected.Metadata["source"].ShouldBe("kept");
    }

    #endregion

    #region Recover

    /// <summary>Non-generic Recover should pass success and recover failures.</summary>
    [Fact]
    public void Recover_Should_Pass_Success_And_Recover_Failures()
    {
        // Arrange
        var success = Result.Ok();
        var failure = Result.Fail(ResultStub.NotFoundError())
            .WithMetadata("source", "kept");

        // Act & Assert
        failure.Recover(null).Errors[0].Code.ShouldBe("result.flow.callback_missing");

        success.Recover(_ => null!).ShouldBeSameAs(success);

        var nullRecovered = failure.Recover(_ => null!);
        nullRecovered.Errors[0].Code.ShouldBe("result.flow.result_missing");

        var recovered = failure.Recover(_ => Result.Ok());
        recovered.IsSuccess.ShouldBeTrue();
        recovered.Metadata["source"].ShouldBe("kept");
    }

    /// <summary>Generic Recover should pass success and recover failures.</summary>
    [Fact]
    public void Generic_Recover_Should_Pass_Success_And_Recover_Failures()
    {
        // Arrange
        var success = Result<int>.Ok(1);
        var failure = Result<int>.Fail(ResultStub.NotFoundError())
            .WithMetadata("source", "kept");

        // Act & Assert
        failure.Recover(null).Errors[0].Code.ShouldBe("result.flow.callback_missing");

        success.Recover(_ => null!).ShouldBeSameAs(success);

        var nullRecovered = failure.Recover(_ => null!);
        nullRecovered.Errors[0].Code.ShouldBe("result.flow.result_missing");

        var recovered = failure.Recover(_ => Result<int>.Ok(42));
        recovered.IsSuccess.ShouldBeTrue();
        recovered.Value.ShouldBe(42);
        recovered.Metadata["source"].ShouldBe("kept");
    }

    #endregion

    #region OrElse

    /// <summary>Non-generic OrElse should pass success and replace failures.</summary>
    [Fact]
    public void OrElse_Should_Pass_Success_And_Replace_Failures()
    {
        // Arrange
        var success = Result.Ok();
        var failure = Result.Fail(ResultStub.NotFoundError())
            .WithMetadata("source", "kept");

        // Act & Assert
        failure.OrElse(null).Errors[0].Code.ShouldBe("result.flow.callback_missing");

        success.OrElse(null!).ShouldBeSameAs(success);

        var nullFallback = failure.OrElse(() => null!);
        nullFallback.Errors[0].Code.ShouldBe("result.flow.result_missing");

        var replaced = failure.OrElse(() => Result.Ok());
        replaced.IsSuccess.ShouldBeTrue();
        replaced.Metadata["source"].ShouldBe("kept");
    }

    /// <summary>Generic OrElse should pass success and replace failures.</summary>
    [Fact]
    public void Generic_OrElse_Should_Pass_Success_And_Replace_Failures()
    {
        // Arrange
        var success = Result<int>.Ok(1);
        var failure = Result<int>.Fail(ResultStub.NotFoundError())
            .WithMetadata("source", "kept");

        // Act & Assert
        failure.OrElse(null).Errors[0].Code.ShouldBe("result.flow.callback_missing");

        success.OrElse(null!).ShouldBeSameAs(success);

        var nullFallback = failure.OrElse(() => null!);
        nullFallback.Errors[0].Code.ShouldBe("result.flow.result_missing");

        var replaced = failure.OrElse(() => Result<int>.Ok(7));
        replaced.IsSuccess.ShouldBeTrue();
        replaced.Value.ShouldBe(7);
        replaced.Metadata["source"].ShouldBe("kept");
    }

    #endregion

    #region Try

    /// <summary>Non-generic Try should guard callbacks and convert exceptions.</summary>
    [Fact]
    public void Try_Should_Guard_Callbacks_And_Convert_Exceptions()
    {
        // Act & Assert
        ResultExtension.Try(null).Errors[0].Code.ShouldBe("result.flow.callback_missing");

        ResultExtension.Try(() => null!).Errors[0].Code.ShouldBe("result.flow.result_missing");

        ResultExtension.Try(() => Result.Ok()).IsSuccess.ShouldBeTrue();

        var mapped = ResultExtension.Try(
            () => throw new InvalidOperationException("boom"));
        mapped.IsFailure.ShouldBeTrue();
        mapped.Errors[0].Code.ShouldBe("result.exception");

        var factoryMapped = ResultExtension.Try(
            () => throw new InvalidOperationException("boom"),
            _ => ResultStub.ConflictError());
        factoryMapped.Errors[0].Code.ShouldBe("test.conflict");

        var nullMapped = ResultExtension.Try(
            () => throw new InvalidOperationException("boom"),
            _ => null!);
        nullMapped.Errors[0].Code.ShouldBe("result.flow.exception_mapping_failed");

        Should.Throw<OperationCanceledException>(
            () => ResultExtension.Try(
                () => throw new OperationCanceledException()));
    }

    /// <summary>Generic Try should guard callbacks and convert exceptions.</summary>
    [Fact]
    public void Generic_Try_Should_Guard_Callbacks_And_Convert_Exceptions()
    {
        // Act & Assert
        ResultExtension.Try<int>(null).Errors[0].Code.ShouldBe("result.flow.callback_missing");

        ResultExtension.Try<int>(() => null!).Errors[0].Code.ShouldBe("result.flow.result_missing");

        ResultExtension.Try<int>(() => Result<int>.Ok(5)).Value.ShouldBe(5);

        var mapped = ResultExtension.Try<int>(
            () => throw new InvalidOperationException("boom"));
        mapped.IsFailure.ShouldBeTrue();
        mapped.Errors[0].Code.ShouldBe("result.exception");

        var factoryMapped = ResultExtension.Try<int>(
            () => throw new InvalidOperationException("boom"),
            _ => ResultStub.ConflictError());
        factoryMapped.Errors[0].Code.ShouldBe("test.conflict");

        var nullMapped = ResultExtension.Try<int>(
            () => throw new InvalidOperationException("boom"),
            _ => null!);
        nullMapped.Errors[0].Code.ShouldBe("result.flow.exception_mapping_failed");

        Should.Throw<OperationCanceledException>(
            () => ResultExtension.Try<int>(
                () => throw new OperationCanceledException()));
    }

    #endregion

    #region MapTry and BindTry

    /// <summary>MapTry should propagate failure, guard callbacks and convert exceptions.</summary>
    [Fact]
    public void MapTry_Should_Propagate_Failure_Guard_Callbacks_And_Convert_Exceptions()
    {
        // Arrange
        var failure = Result<int>.Fail(ResultStub.NotFoundError())
            .WithMetadata("source", "kept");
        var success = Result<int>.Ok(3)
            .WithMetadata("source", "kept");

        // Act & Assert
        var propagated = failure.MapTry(v => v * 2);
        propagated.IsFailure.ShouldBeTrue();
        propagated.Metadata["source"].ShouldBe("kept");

        success.MapTry((Func<int, int>?)null)
            .Errors[0].Code.ShouldBe("result.flow.callback_missing");

        success.MapTry(v => v * 2).Value.ShouldBe(6);

        success.MapTry<int, int>(_ => throw new InvalidOperationException())
            .Errors[0].Code.ShouldBe("result.exception");

        success.MapTry<int, int>(
                _ => throw new InvalidOperationException(),
                _ => ResultStub.ConflictError())
            .Errors[0].Code.ShouldBe("test.conflict");

        Should.Throw<OperationCanceledException>(
            () => success.MapTry<int, int>(
                _ => throw new OperationCanceledException()));
    }

    /// <summary>BindTry should propagate failure, guard callbacks and convert exceptions.</summary>
    [Fact]
    public void BindTry_Should_Propagate_Failure_Guard_Callbacks_And_Convert_Exceptions()
    {
        // Arrange
        var failure = Result<int>.Fail(ResultStub.NotFoundError())
            .WithMetadata("source", "kept");
        var success = Result<int>.Ok(3)
            .WithMetadata("source", "kept");

        // Act & Assert
        var propagated = failure.BindTry(v => Result<string>.Ok(v.ToString()));
        propagated.IsFailure.ShouldBeTrue();
        propagated.Metadata["source"].ShouldBe("kept");

        success.BindTry((Func<int, Result<string>>?)null)
            .Errors[0].Code.ShouldBe("result.flow.callback_missing");

        success.BindTry<int, string>(_ => null!)
            .Errors[0].Code.ShouldBe("result.flow.result_missing");

        success.BindTry(v => Result<string>.Ok(v.ToString())).Value.ShouldBe("3");

        success.BindTry<int, string>(_ => throw new InvalidOperationException())
            .Errors[0].Code.ShouldBe("result.exception");

        success.BindTry<int, string>(
                _ => throw new InvalidOperationException(),
                _ => ResultStub.ConflictError())
            .Errors[0].Code.ShouldBe("test.conflict");

        Should.Throw<OperationCanceledException>(
            () => success.BindTry<int, string>(
                _ => throw new OperationCanceledException()));
    }

    #endregion

    #region Effects

    /// <summary>Tap should invoke the action only on success.</summary>
    [Fact]
    public void Tap_Should_Invoke_Only_On_Success()
    {
        // Arrange
        var invoked = 0;
        var success = Result.Ok();
        var failure = Result.Fail(ResultStub.NotFoundError());

        // Act & Assert
        success.Tap(() => invoked++).ShouldBeSameAs(success);
        success.Tap((Action?)null).ShouldBeSameAs(success);
        failure.Tap(() => invoked++).ShouldBeSameAs(failure);
        invoked.ShouldBe(1);
    }

    /// <summary>Generic Tap should invoke the action only on success.</summary>
    [Fact]
    public void Generic_Tap_Should_Invoke_Only_On_Success()
    {
        // Arrange
        var seen = 0;
        var success = Result<int>.Ok(5);
        var failure = Result<int>.Fail(ResultStub.NotFoundError());

        // Act & Assert
        success.Tap(v => seen = v).ShouldBeSameAs(success);
        success.Tap((Action<int>?)null).ShouldBeSameAs(success);
        failure.Tap(v => seen = v).ShouldBeSameAs(failure);
        seen.ShouldBe(5);
    }

    /// <summary>TapError should invoke the action only on failure.</summary>
    [Fact]
    public void TapError_Should_Invoke_Only_On_Failure()
    {
        // Arrange
        List<Error>? seen = null;
        var success = Result.Ok();
        var failure = Result.Fail(ResultStub.NotFoundError());

        // Act & Assert
        success.TapError(errors => seen = errors).ShouldBeSameAs(success);
        failure.TapError(errors => seen = errors).ShouldBeSameAs(failure);
        failure.TapError((Action<List<Error>>?)null).ShouldBeSameAs(failure);
        seen.ShouldNotBeNull();
        seen[0].Code.ShouldBe("test.not_found");
    }

    /// <summary>Generic TapError should invoke the action only on failure.</summary>
    [Fact]
    public void Generic_TapError_Should_Invoke_Only_On_Failure()
    {
        // Arrange
        List<Error>? seen = null;
        var success = Result<int>.Ok(1);
        var failure = Result<int>.Fail(ResultStub.NotFoundError());

        // Act & Assert
        success.TapError(errors => seen = errors).ShouldBeSameAs(success);
        failure.TapError(errors => seen = errors).ShouldBeSameAs(failure);
        failure.TapError((Action<List<Error>>?)null).ShouldBeSameAs(failure);
        seen.ShouldNotBeNull();
        seen[0].Code.ShouldBe("test.not_found");
    }

    /// <summary>Switch should select the side effect by state.</summary>
    [Fact]
    public void Switch_Should_Select_Side_Effect_By_State()
    {
        // Arrange
        var success = Result.Ok();
        var failure = Result.Fail(ResultStub.NotFoundError());
        var invoked = string.Empty;

        // Act & Assert
        success.Switch(() => invoked = "ok", _ => invoked = "fail")
            .ShouldBeSameAs(success);
        invoked.ShouldBe("ok");

        failure.Switch(() => invoked = "ok", _ => invoked = "fail")
            .ShouldBeSameAs(failure);
        invoked.ShouldBe("fail");

        success.Switch(null, null).ShouldBeSameAs(success);
        failure.Switch(null, null).ShouldBeSameAs(failure);
    }

    /// <summary>Generic Switch should select the side effect by state.</summary>
    [Fact]
    public void Generic_Switch_Should_Select_Side_Effect_By_State()
    {
        // Arrange
        var success = Result<int>.Ok(5);
        var failure = Result<int>.Fail(ResultStub.NotFoundError());
        var invoked = string.Empty;

        // Act & Assert
        success.Switch(v => invoked = $"ok:{v}", _ => invoked = "fail")
            .ShouldBeSameAs(success);
        invoked.ShouldBe("ok:5");

        failure.Switch(v => invoked = $"ok:{v}", _ => invoked = "fail")
            .ShouldBeSameAs(failure);
        invoked.ShouldBe("fail");

        success.Switch(null, null).ShouldBeSameAs(success);
        failure.Switch(null, null).ShouldBeSameAs(failure);
    }

    #endregion

    #region Combination

    /// <summary>Combine should succeed for empty and all-success input.</summary>
    [Fact]
    public void Combine_Should_Succeed_For_Empty_And_All_Success_Input()
    {
        // Act & Assert
        ResultExtension.Combine().IsSuccess.ShouldBeTrue();
        ResultExtension.Combine(Result.Ok(), Result<int>.Ok(1)).IsSuccess.ShouldBeTrue();
    }

    /// <summary>Combine should report a null element.</summary>
    [Fact]
    public void Combine_With_Null_Element_Should_Report_Result_Missing()
    {
        // Act
        var combined = ResultExtension.Combine((Result)null!);

        // Assert
        combined.IsFailure.ShouldBeTrue();
        combined.Errors[0].Code.ShouldBe("result.flow.result_missing");
    }

    /// <summary>Combine should merge same-status failures.</summary>
    [Fact]
    public void Combine_With_Same_Status_Failures_Should_Merge_Errors()
    {
        // Arrange
        var first = Result.Fail(ResultStub.NotFoundError());
        var second = Result.Fail(ResultStub.NotFoundError());

        // Act
        var combined = ResultExtension.Combine(first, second);

        // Assert
        combined.IsFailure.ShouldBeTrue();
        combined.StatusCode.ShouldBe(ResultConstant.StatusCode.NotFound);
        combined.Errors.Count.ShouldBe(2);
    }

    /// <summary>Combine should report mixed failure statuses.</summary>
    [Fact]
    public void Combine_With_Mixed_Status_Failures_Should_Report_Mixed()
    {
        // Arrange
        var first = Result.Fail(ResultStub.NotFoundError());
        var second = Result.Fail(ResultStub.ConflictError());

        // Act
        var combined = ResultExtension.Combine(first, second);

        // Assert
        combined.IsFailure.ShouldBeTrue();
        combined.Errors[0].Code.ShouldBe("result.errors.mixed_status_codes");
    }

    #endregion

    #region Extraction

    /// <summary>ValueOr should return the value or the fallback.</summary>
    [Fact]
    public void ValueOr_Should_Return_Value_Or_Fallback()
    {
        // Arrange
        var success = Result<int>.Ok(5);
        var failure = Result<int>.Fail(ResultStub.NotFoundError());

        // Act & Assert
        success.ValueOr(9).ShouldBe(5);
        failure.ValueOr(9).ShouldBe(9);
        failure.ValueOr().ShouldBe(default);
    }

    /// <summary>ValueOrThrow should return the value or throw a ResultException.</summary>
    [Fact]
    public void ValueOrThrow_Should_Return_Value_Or_Throw()
    {
        // Arrange
        var success = Result<int>.Ok(5);
        var failure = Result<int>.Fail(ResultStub.NotFoundError());

        // Act & Assert
        success.ValueOrThrow().ShouldBe(5);

        var exception = Should.Throw<ResultException>(() => failure.ValueOrThrow());
        exception.Errors[0].Code.ShouldBe("test.not_found");
        exception.StatusCode.ShouldBe(ResultConstant.StatusCode.NotFound);
    }

    /// <summary>ThrowIfFailure should pass successes and throw on failures.</summary>
    [Fact]
    public void ThrowIfFailure_Should_Pass_Success_And_Throw_On_Failure()
    {
        // Arrange
        var success = Result.Ok();
        var failure = Result.Fail(ResultStub.NotFoundError());
        var genericSuccess = Result<int>.Ok(1);
        var genericFailure = Result<int>.Fail(ResultStub.NotFoundError());

        // Act & Assert
        success.ThrowIfFailure().ShouldBeSameAs(success);
        genericSuccess.ThrowIfFailure().ShouldBeSameAs(genericSuccess);

        Should.Throw<ResultException>(() => failure.ThrowIfFailure());
        Should.Throw<ResultException>(() => genericFailure.ThrowIfFailure());
    }

    #endregion
}
