using BuildingBlock.Monad.Errors;
using BuildingBlock.Monad.Results;

namespace BuildingBlock.Monad.UnitTest.Results;

/// <summary>Tests for the asynchronous flow extensions in <see cref="ResultExtension"/>.</summary>
[Trait("Category", "Unit")]
public class ResultFlowAsyncSpec
{
    #region MatchAsync and FoldAsync

    /// <summary>MatchAsync legacy overload should select the callback by state.</summary>
    [Fact]
    public async Task MatchAsync_Legacy_Should_Select_Callback_By_State()
    {
        // Arrange
        var token = TestContext.Current.CancellationToken;
        var success = Result<int>.Ok(7);
        var failure = Result<int>.Fail(ResultStub.NotFoundError());

        // Act & Assert
        var ok = await success.MatchAsync(
            v => Task.FromResult($"ok:{v}"),
            _ => Task.FromResult("fail"),
            token);
        ok.ShouldBe("ok:7");

        var failed = await failure.MatchAsync(
            _ => Task.FromResult("ok"),
            errors => Task.FromResult(errors[0].Code),
            token);
        failed.ShouldBe("test.not_found");

        (await success.MatchAsync<int, string>(
            (Func<int, Task<string>>?)null,
            _ => Task.FromResult("fail"),
            token)).ShouldBeNull();

        (await failure.MatchAsync<int, string>(
            _ => Task.FromResult("ok"),
            (Func<List<Error>, Task<string>>?)null,
            token)).ShouldBeNull();
    }

    /// <summary>MatchAsync legacy overload should default when a callback returns a null task.</summary>
    [Fact]
    public async Task MatchAsync_Legacy_Should_Default_When_Task_Is_Null()
    {
        // Arrange
        var token = TestContext.Current.CancellationToken;
        var success = Result<int>.Ok(7);
        var failure = Result<int>.Fail(ResultStub.NotFoundError());

        // Act & Assert
        (await success.MatchAsync<int, string>(
            _ => null!,
            _ => Task.FromResult("fail"),
            token)).ShouldBeNull();

        (await failure.MatchAsync<int, string>(
            _ => Task.FromResult("ok"),
            _ => null!,
            token)).ShouldBeNull();
    }

    /// <summary>MatchAsync token-aware overload should forward the token and select the callback.</summary>
    [Fact]
    public async Task MatchAsync_TokenAware_Should_Forward_Token_And_Select_Callback()
    {
        // Arrange
        var token = TestContext.Current.CancellationToken;
        var success = Result<int>.Ok(7);
        var failure = Result<int>.Fail(ResultStub.NotFoundError());
        var received = CancellationToken.None;

        // Act & Assert
        var ok = await success.MatchAsync(
            (v, ct) =>
            {
                received = ct;
                return Task.FromResult($"ok:{v}");
            },
            (_, ct) =>
            {
                received = ct;
                return Task.FromResult("fail");
            },
            token);
        ok.ShouldBe("ok:7");
        received.ShouldBe(token);

        var failed = await failure.MatchAsync(
            (_, ct) =>
            {
                received = ct;
                return Task.FromResult("ok");
            },
            (errors, ct) =>
            {
                received = ct;
                return Task.FromResult(errors[0].Code);
            },
            token);
        failed.ShouldBe("test.not_found");
        received.ShouldBe(token);

        (await success.MatchAsync<int, string>(
            (Func<int, CancellationToken, Task<string>>?)null,
            (_, _) => Task.FromResult("fail"),
            token)).ShouldBeNull();

        (await failure.MatchAsync<int, string>(
            (_, _) => Task.FromResult("ok"),
            (Func<List<Error>, CancellationToken, Task<string>>?)null,
            token)).ShouldBeNull();
    }

    /// <summary>FoldAsync should behave like MatchAsync.</summary>
    [Fact]
    public async Task FoldAsync_Should_Behave_Like_MatchAsync()
    {
        // Arrange
        var token = TestContext.Current.CancellationToken;
        var success = Result<int>.Ok(7);
        var failure = Result<int>.Fail(ResultStub.NotFoundError());

        // Act & Assert
        (await success.FoldAsync(
            v => Task.FromResult(v * 2),
            _ => Task.FromResult(-1),
            token)).ShouldBe(14);

        (await failure.FoldAsync(
            (v, _) => Task.FromResult(v * 2),
            (errors, _) => Task.FromResult(errors.Count),
            token)).ShouldBe(1);
    }

    #endregion

    #region MapAsync

    /// <summary>MapAsync legacy overload should propagate failure, guard callbacks and map success.</summary>
    [Fact]
    public async Task MapAsync_Legacy_Should_Propagate_Failure_Guard_And_Map()
    {
        // Arrange
        var token = TestContext.Current.CancellationToken;
        var failure = Result<int>.Fail(ResultStub.NotFoundError())
            .WithMetadata("source", "kept");
        var success = Result<int>.Ok(7)
            .WithMetadata("source", "kept");

        // Act & Assert
        var propagated = await failure.MapAsync(v => Task.FromResult(v + 1), token);
        propagated.IsFailure.ShouldBeTrue();
        propagated.StatusCode.ShouldBe(ResultConstant.StatusCode.NotFound);
        propagated.Metadata["source"].ShouldBe("kept");

        var missing = await success.MapAsync<int, int>((Func<int, Task<int>>?)null, token);
        missing.Errors[0].Code.ShouldBe("result.flow.callback_missing");

        var noTask = await success.MapAsync<int, int>(_ => null!, token);
        noTask.Errors[0].Code.ShouldBe("result.flow.task_missing");

        var mapped = await success.MapAsync(v => Task.FromResult(v + 1), token);
        mapped.IsSuccess.ShouldBeTrue();
        mapped.Value.ShouldBe(8);
        mapped.Metadata["source"].ShouldBe("kept");
    }

    /// <summary>MapAsync token-aware overload should forward the token.</summary>
    [Fact]
    public async Task MapAsync_TokenAware_Should_Forward_Token()
    {
        // Arrange
        var token = TestContext.Current.CancellationToken;
        var success = Result<int>.Ok(7);
        var received = CancellationToken.None;

        // Act
        var mapped = await success.MapAsync(
            (v, ct) =>
            {
                received = ct;
                return Task.FromResult(v + 1);
            },
            token);

        // Assert
        mapped.Value.ShouldBe(8);
        received.ShouldBe(token);

        (await success.MapAsync<int, int>(
            (Func<int, CancellationToken, Task<int>>?)null,
            token)).Errors[0].Code.ShouldBe("result.flow.callback_missing");
    }

    #endregion

    #region BindAsync

    /// <summary>BindAsync legacy overload should propagate failure, guard callbacks and bind success.</summary>
    [Fact]
    public async Task BindAsync_Legacy_Should_Propagate_Failure_Guard_And_Bind()
    {
        // Arrange
        var token = TestContext.Current.CancellationToken;
        var failure = Result<int>.Fail(ResultStub.NotFoundError())
            .WithMetadata("source", "kept");
        var success = Result<int>.Ok(7)
            .WithMetadata("source", "kept");

        // Act & Assert
        var propagated = await failure.BindAsync(
            v => Task.FromResult(Result<string>.Ok(v.ToString())),
            token);
        propagated.IsFailure.ShouldBeTrue();
        propagated.StatusCode.ShouldBe(ResultConstant.StatusCode.NotFound);
        propagated.Metadata["source"].ShouldBe("kept");

        var missing = await success.BindAsync<int, string>((Func<int, Task<Result<string>>>?)null, token);
        missing.Errors[0].Code.ShouldBe("result.flow.callback_missing");

        var noTask = await success.BindAsync<int, string>(_ => null!, token);
        noTask.Errors[0].Code.ShouldBe("result.flow.task_missing");

        var noResult = await success.BindAsync(
            _ => Task.FromResult<Result<string>>(null!),
            token);
        noResult.Errors[0].Code.ShouldBe("result.flow.result_missing");

        var bound = await success.BindAsync(
            v => Task.FromResult(Result<string>.Ok($"value-{v}")),
            token);
        bound.IsSuccess.ShouldBeTrue();
        bound.Value.ShouldBe("value-7");
        bound.Metadata["source"].ShouldBe("kept");
    }

    /// <summary>BindAsync token-aware overload should forward the token.</summary>
    [Fact]
    public async Task BindAsync_TokenAware_Should_Forward_Token()
    {
        // Arrange
        var token = TestContext.Current.CancellationToken;
        var success = Result<int>.Ok(7);
        var received = CancellationToken.None;

        // Act
        var bound = await success.BindAsync(
            (v, ct) =>
            {
                received = ct;
                return Task.FromResult(Result<string>.Ok(v.ToString()));
            },
            token);

        // Assert
        bound.Value.ShouldBe("7");
        received.ShouldBe(token);

        (await success.BindAsync<int, string>(
            (Func<int, CancellationToken, Task<Result<string>>>)null!,
            token)).Errors[0].Code.ShouldBe("result.flow.callback_missing");
    }

    #endregion

    #region EnsureAsync

    /// <summary>EnsureAsync legacy overload should pass failures and enforce the predicate.</summary>
    [Fact]
    public async Task EnsureAsync_Legacy_Should_Pass_Failures_And_Enforce_Predicate()
    {
        // Arrange
        var token = TestContext.Current.CancellationToken;
        var failure = Result<int>.Fail(ResultStub.NotFoundError());
        var success = Result<int>.Ok(7)
            .WithMetadata("source", "kept");
        var guard = ResultStub.ConflictError();

        // Act & Assert
        (await failure.EnsureAsync<int>((Func<int, Task<bool>>?)null, guard, token))
            .ShouldBeSameAs(failure);

        (await success.EnsureAsync<int>((Func<int, Task<bool>>?)null, guard, token))
            .Errors[0].Code.ShouldBe("result.flow.callback_missing");

        (await success.EnsureAsync<int>(_ => null!, guard, token))
            .Errors[0].Code.ShouldBe("result.flow.task_missing");

        (await success.EnsureAsync(v => Task.FromResult(v > 0), guard, token))
            .ShouldBeSameAs(success);

        var rejected = await success.EnsureAsync(
            v => Task.FromResult(v < 0), guard, token);
        rejected.IsFailure.ShouldBeTrue();
        rejected.Errors[0].Code.ShouldBe("test.conflict");
        rejected.Metadata["source"].ShouldBe("kept");
    }

    /// <summary>EnsureAsync token-aware overload should forward the token.</summary>
    [Fact]
    public async Task EnsureAsync_TokenAware_Should_Forward_Token()
    {
        // Arrange
        var token = TestContext.Current.CancellationToken;
        var success = Result<int>.Ok(7);
        var guard = ResultStub.ConflictError();
        var received = CancellationToken.None;

        // Act
        var ensured = await success.EnsureAsync(
            (v, ct) =>
            {
                received = ct;
                return Task.FromResult(v > 0);
            },
            guard,
            token);

        // Assert
        ensured.ShouldBeSameAs(success);
        received.ShouldBe(token);

        (await success.EnsureAsync<int>(
            (Func<int, CancellationToken, Task<bool>>?)null,
            guard,
            token)).Errors[0].Code.ShouldBe("result.flow.callback_missing");
    }

    #endregion

    #region RecoverAsync

    /// <summary>RecoverAsync legacy overload should pass success and recover failures.</summary>
    [Fact]
    public async Task RecoverAsync_Legacy_Should_Pass_Success_And_Recover()
    {
        // Arrange
        var token = TestContext.Current.CancellationToken;
        var success = Result<int>.Ok(1);
        var failure = Result<int>.Fail(ResultStub.NotFoundError())
            .WithMetadata("source", "kept");

        // Act & Assert
        (await success.RecoverAsync<int>((Func<List<Error>, Task<int>>?)null, token))
            .ShouldBeSameAs(success);

        (await failure.RecoverAsync<int>((Func<List<Error>, Task<int>>?)null, token))
            .Errors[0].Code.ShouldBe("result.flow.callback_missing");

        (await failure.RecoverAsync<int>(_ => null!, token))
            .Errors[0].Code.ShouldBe("result.flow.task_missing");

        var recovered = await failure.RecoverAsync(
            _ => Task.FromResult(42),
            token);
        recovered.IsSuccess.ShouldBeTrue();
        recovered.Value.ShouldBe(42);
        recovered.Metadata["source"].ShouldBe("kept");
    }

    /// <summary>RecoverAsync token-aware overload should forward the token.</summary>
    [Fact]
    public async Task RecoverAsync_TokenAware_Should_Forward_Token()
    {
        // Arrange
        var token = TestContext.Current.CancellationToken;
        var failure = Result<int>.Fail(ResultStub.NotFoundError());
        var received = CancellationToken.None;

        // Act
        var recovered = await failure.RecoverAsync(
            (_, ct) =>
            {
                received = ct;
                return Task.FromResult(42);
            },
            token);

        // Assert
        recovered.Value.ShouldBe(42);
        received.ShouldBe(token);

        (await failure.RecoverAsync<int>(
            (Func<List<Error>, CancellationToken, Task<int>>?)null,
            token)).Errors[0].Code.ShouldBe("result.flow.callback_missing");
    }

    #endregion

    #region OrElseAsync

    /// <summary>OrElseAsync legacy overload should pass success and replace failures.</summary>
    [Fact]
    public async Task OrElseAsync_Legacy_Should_Pass_Success_And_Replace()
    {
        // Arrange
        var token = TestContext.Current.CancellationToken;
        var success = Result<int>.Ok(1);
        var failure = Result<int>.Fail(ResultStub.NotFoundError())
            .WithMetadata("source", "kept");

        // Act & Assert
        (await success.OrElseAsync<int>((Func<List<Error>, Task<Result<int>>>?)null, token))
            .ShouldBeSameAs(success);

        (await failure.OrElseAsync<int>((Func<List<Error>, Task<Result<int>>>?)null, token))
            .Errors[0].Code.ShouldBe("result.flow.callback_missing");

        (await failure.OrElseAsync<int>(_ => null!, token))
            .Errors[0].Code.ShouldBe("result.flow.task_missing");

        var noResult = await failure.OrElseAsync(
            _ => Task.FromResult<Result<int>>(null!),
            token);
        noResult.Errors[0].Code.ShouldBe("result.flow.result_missing");

        var replaced = await failure.OrElseAsync(
            _ => Task.FromResult(Result<int>.Ok(7)),
            token);
        replaced.IsSuccess.ShouldBeTrue();
        replaced.Value.ShouldBe(7);
        replaced.Metadata["source"].ShouldBe("kept");
    }

    /// <summary>OrElseAsync token-aware overload should forward the token.</summary>
    [Fact]
    public async Task OrElseAsync_TokenAware_Should_Forward_Token()
    {
        // Arrange
        var token = TestContext.Current.CancellationToken;
        var failure = Result<int>.Fail(ResultStub.NotFoundError());
        var received = CancellationToken.None;

        // Act
        var replaced = await failure.OrElseAsync(
            (_, ct) =>
            {
                received = ct;
                return Task.FromResult(Result<int>.Ok(7));
            },
            token);

        // Assert
        replaced.Value.ShouldBe(7);
        received.ShouldBe(token);

        (await failure.OrElseAsync<int>(
            (Func<List<Error>, CancellationToken, Task<Result<int>>>)null!,
            token)).Errors[0].Code.ShouldBe("result.flow.callback_missing");
    }

    #endregion

    #region TapAsync

    /// <summary>TapAsync legacy overload should invoke the side effect only on success.</summary>
    [Fact]
    public async Task TapAsync_Legacy_Should_Invoke_Only_On_Success()
    {
        // Arrange
        var token = TestContext.Current.CancellationToken;
        var seen = 0;
        var success = Result<int>.Ok(5);
        var failure = Result<int>.Fail(ResultStub.NotFoundError());

        // Act & Assert
        (await success.TapAsync(v => Task.FromResult(seen = v), token))
            .ShouldBeSameAs(success);
        seen.ShouldBe(5);

        (await success.TapAsync<int>((Func<int, Task>?)null, token)).ShouldBeSameAs(success);

        (await success.TapAsync<int>(_ => null!, token))
            .ShouldBeSameAs(success);

        (await failure.TapAsync(v => Task.FromResult(seen = v), token))
            .ShouldBeSameAs(failure);
        seen.ShouldBe(5);
    }

    /// <summary>TapAsync token-aware overload should forward the token.</summary>
    [Fact]
    public async Task TapAsync_TokenAware_Should_Forward_Token()
    {
        // Arrange
        var token = TestContext.Current.CancellationToken;
        var success = Result<int>.Ok(5);
        var received = CancellationToken.None;

        // Act & Assert
        (await success.TapAsync(
            (v, ct) =>
            {
                received = ct;
                return Task.CompletedTask;
            },
            token)).ShouldBeSameAs(success);
        received.ShouldBe(token);

        (await success.TapAsync(
            (_, _) => (Task)null!,
            token)).ShouldBeSameAs(success);

        (await success.TapAsync<int>(
            (Func<int, CancellationToken, Task>?)null,
            token)).ShouldBeSameAs(success);
    }

    #endregion

    #region TapErrorAsync

    /// <summary>TapErrorAsync legacy overload should invoke the side effect only on failure.</summary>
    [Fact]
    public async Task TapErrorAsync_Legacy_Should_Invoke_Only_On_Failure()
    {
        // Arrange
        var token = TestContext.Current.CancellationToken;
        List<Error>? seen = null;
        var success = Result<int>.Ok(1);
        var failure = Result<int>.Fail(ResultStub.NotFoundError());

        // Act & Assert
        (await success.TapErrorAsync(errors => Task.FromResult(seen = errors), token))
            .ShouldBeSameAs(success);
        seen.ShouldBeNull();

        (await failure.TapErrorAsync(errors => Task.FromResult(seen = errors), token))
            .ShouldBeSameAs(failure);
        seen.ShouldNotBeNull();
        seen![0].Code.ShouldBe("test.not_found");

        (await failure.TapErrorAsync<int>((Func<List<Error>, Task>?)null, token)).ShouldBeSameAs(failure);

        (await failure.TapErrorAsync<int>(_ => null!, token))
            .ShouldBeSameAs(failure);
    }

    /// <summary>TapErrorAsync token-aware overload should forward the token.</summary>
    [Fact]
    public async Task TapErrorAsync_TokenAware_Should_Forward_Token()
    {
        // Arrange
        var token = TestContext.Current.CancellationToken;
        var failure = Result<int>.Fail(ResultStub.NotFoundError());
        var received = CancellationToken.None;

        // Act & Assert
        (await failure.TapErrorAsync(
            (_, ct) =>
            {
                received = ct;
                return Task.CompletedTask;
            },
            token)).ShouldBeSameAs(failure);
        received.ShouldBe(token);

        (await failure.TapErrorAsync(
            (_, _) => (Task)null!,
            token)).ShouldBeSameAs(failure);

        (await failure.TapErrorAsync<int>(
            (Func<List<Error>, CancellationToken, Task>?)null,
            token)).ShouldBeSameAs(failure);
    }

    #endregion

    #region SwitchAsync

    /// <summary>SwitchAsync legacy overload should select the side effect by state.</summary>
    [Fact]
    public async Task SwitchAsync_Legacy_Should_Select_Side_Effect_By_State()
    {
        // Arrange
        var token = TestContext.Current.CancellationToken;
        var success = Result.Ok();
        var failure = Result.Fail(ResultStub.NotFoundError());
        var invoked = string.Empty;

        // Act & Assert
        await success.SwitchAsync(
            () => Task.FromResult(invoked = "ok"),
            _ => Task.FromResult(invoked = "fail"),
            token);
        invoked.ShouldBe("ok");

        await failure.SwitchAsync(
            () => Task.FromResult(invoked = "ok"),
            _ => Task.FromResult(invoked = "fail"),
            token);
        invoked.ShouldBe("fail");

        await success.SwitchAsync(
            (Func<Task>?)null,
            (Func<List<Error>, Task>?)null,
            token);
        await failure.SwitchAsync(
            (Func<Task>?)null,
            (Func<List<Error>, Task>?)null,
            token);
    }

    /// <summary>SwitchAsync legacy overload should tolerate null tasks.</summary>
    [Fact]
    public async Task SwitchAsync_Legacy_Should_Tolerate_Null_Tasks()
    {
        // Arrange
        var token = TestContext.Current.CancellationToken;
        var success = Result.Ok();
        var failure = Result.Fail(ResultStub.NotFoundError());

        // Arrange
        Func<Task> nullSuccessTask = () => null!;
        Func<List<Error>, Task> nullFailureTask = _ => null!;

        // Act & Assert
        await success.SwitchAsync(
            nullSuccessTask, _ => Task.CompletedTask, token);
        await failure.SwitchAsync(
            () => Task.CompletedTask, nullFailureTask, token);
    }

    /// <summary>SwitchAsync token-aware overload should forward the token.</summary>
    [Fact]
    public async Task SwitchAsync_TokenAware_Should_Forward_Token()
    {
        // Arrange
        var token = TestContext.Current.CancellationToken;
        var success = Result.Ok();
        var failure = Result.Fail(ResultStub.NotFoundError());
        var received = CancellationToken.None;
        var invoked = string.Empty;

        // Act & Assert
        await success.SwitchAsync(
            ct =>
            {
                received = ct;
                return Task.FromResult(invoked = "ok");
            },
            (_, _) => Task.FromResult(invoked = "fail"),
            token);
        invoked.ShouldBe("ok");
        received.ShouldBe(token);

        await failure.SwitchAsync(
            _ => Task.FromResult(invoked = "ok"),
            (errors, ct) =>
            {
                received = ct;
                return Task.FromResult(invoked = errors[0].Code);
            },
            token);
        invoked.ShouldBe("test.not_found");
        received.ShouldBe(token);

        await success.SwitchAsync(
            (Func<CancellationToken, Task>?)null,
            (Func<List<Error>, CancellationToken, Task>?)null,
            token);

        await success.SwitchAsync(
            _ => null!,
            (_, _) => Task.CompletedTask,
            token);
    }

    #endregion

    #region TryAsync Generic

    /// <summary>TryAsync legacy overload should guard callbacks and convert exceptions.</summary>
    [Fact]
    public async Task TryAsync_Generic_Legacy_Should_Guard_And_Convert()
    {
        // Arrange
        var token = TestContext.Current.CancellationToken;

        // Act & Assert
        (await ResultExtension.TryAsync<int>(
            (Func<Task<int>>)null!, null, token))
            .Errors[0].Code.ShouldBe("result.flow.callback_missing");

        Func<Task<int>> noTask = () => null!;
        (await noTask.TryAsync(null, token))
            .Errors[0].Code.ShouldBe("result.flow.task_missing");

        Func<Task<int>> success = () => Task.FromResult(5);
        (await success.TryAsync(null, token)).Value.ShouldBe(5);

        Func<Task<int>> throws = () => throw new InvalidOperationException();
        (await throws.TryAsync(null, token))
            .Errors[0].Code.ShouldBe("result.exception");

        (await throws.TryAsync(_ => ResultStub.ConflictError(), token))
            .Errors[0].Code.ShouldBe("test.conflict");

        (await throws.TryAsync(_ => null!, token))
            .Errors[0].Code.ShouldBe("result.flow.exception_mapping_failed");

        Func<Task<int>> cancelled = () => throw new OperationCanceledException();
        await Should.ThrowAsync<OperationCanceledException>(
            () => cancelled.TryAsync(null, token));
    }

    /// <summary>TryAsync token-aware overload should forward the token.</summary>
    [Fact]
    public async Task TryAsync_Generic_TokenAware_Should_Forward_Token()
    {
        // Arrange
        var token = TestContext.Current.CancellationToken;
        var received = CancellationToken.None;
        var called = false;

        // Act
        var result = await ((Func<CancellationToken, Task<int>>)(async ct =>
        {
            called = true;
            received = ct;
            await Task.Yield();
            return 5;
        })).TryAsync(null, token);

        // Assert
        result.Value.ShouldBe(5);
        called.ShouldBeTrue();
        received.ShouldBe(token);

        (await ((Func<CancellationToken, Task<int>>)(_ => null!)).TryAsync(
            null, token))
            .Errors[0].Code.ShouldBe("result.flow.task_missing");
    }

    #endregion

    #region TryAsync Non-Generic

    /// <summary>TryAsync non-generic legacy overload should guard callbacks and convert exceptions.</summary>
    [Fact]
    public async Task TryAsync_NonGeneric_Legacy_Should_Guard_And_Convert()
    {
        // Arrange
        var token = TestContext.Current.CancellationToken;

        // Act & Assert
        (await ResultExtension.TryAsync(
            (Func<Task>?)null, null, token))
            .Errors[0].Code.ShouldBe("result.flow.callback_missing");

        Func<Task> noTask = () => null!;
        (await noTask.TryAsync(null, token))
            .Errors[0].Code.ShouldBe("result.exception");

        var completed = false;
        Func<Task> success = () =>
        {
            completed = true;
            return Task.CompletedTask;
        };
        (await success.TryAsync(null, token)).IsSuccess.ShouldBeTrue();
        completed.ShouldBeTrue();

        Func<Task> throws = () => throw new InvalidOperationException();
        (await throws.TryAsync(null, token))
            .Errors[0].Code.ShouldBe("result.exception");

        (await throws.TryAsync(_ => ResultStub.ConflictError(), token))
            .Errors[0].Code.ShouldBe("test.conflict");

        (await throws.TryAsync(_ => null!, token))
            .Errors[0].Code.ShouldBe("result.flow.exception_mapping_failed");

        Func<Task> cancelled = () => throw new OperationCanceledException();
        await Should.ThrowAsync<OperationCanceledException>(
            () => cancelled.TryAsync(null, token));
    }

    /// <summary>TryAsync non-generic token-aware overload should forward the token.</summary>
    [Fact]
    public async Task TryAsync_NonGeneric_TokenAware_Should_Forward_Token()
    {
        // Arrange
        var token = TestContext.Current.CancellationToken;
        var received = CancellationToken.None;

        // Act
        var result = await ((Func<CancellationToken, Task>)(async ct =>
        {
            received = ct;
            await Task.Yield();
        })).TryAsync(null, token);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        received.ShouldBe(token);

        (await ((Func<CancellationToken, Task>)(_ => null!)).TryAsync(
            null, token))
            .Errors[0].Code.ShouldBe("result.flow.task_missing");
    }

    #endregion

    #region Cancellation

    /// <summary>A pre-cancelled token should cancel the flow before callbacks run.</summary>
    [Fact]
    public async Task PreCancelled_Token_Should_Cancel_Flows_Before_Callbacks_Run()
    {
        // Arrange
        var cancelled = new CancellationToken(canceled: true);
        var called = false;
        var success = Result<int>.Ok(1);

        // Act & Assert
        await Should.ThrowAsync<OperationCanceledException>(
            () => success.MapAsync(v => Task.FromResult(called = true), cancelled));
        await Should.ThrowAsync<OperationCanceledException>(
            () => success.MatchAsync(
                v => Task.FromResult(called = true),
                _ => Task.FromResult(false),
                cancelled));
        await Should.ThrowAsync<OperationCanceledException>(
            () => success.EnsureAsync(
                v => Task.FromResult(called = true),
                ResultStub.ConflictError(),
                cancelled));
        await Should.ThrowAsync<OperationCanceledException>(
            () => ((Func<Task<int>>)(() => Task.FromResult(1))).TryAsync(
                null, cancelled));

        called.ShouldBeFalse();
    }

    #endregion
}
