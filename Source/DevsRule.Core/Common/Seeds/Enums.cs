namespace DevsRule.Core.Common.Seeds;

public enum EventWhenType : int
{ 
    Never               = 0,
    OnSuccessOrFailure  = 1,
    OnSuccess           = 2,
    OnFailure           = 3,
    
}

public enum PublishTo : int
{
    All             = 0,
    Subscribers     = 1,
    Registrations   = 2
}

public enum PublishMethod : int
{
    FireAndForget = 0,
    WaitForAll    = 1  
}