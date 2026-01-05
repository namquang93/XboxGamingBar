namespace Shared.Data
{
    public enum SharedAppServiceResponseStatus
    {
        Success,
        Failure,
        ResourceLimitsExceeded,
        Unknown,
        RemoteSystemUnavailable,
        MessageSizeTooLarge,
        AppUnavailable,
        AuthenticationError,
        NetworkNotAvailable,
        DisabledByPolicy,
        WebServiceUnavailable
    }
}
