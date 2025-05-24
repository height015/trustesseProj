using System;

namespace trustesseApp.Infrastructure.Helpers;

public class DataResponseObj<TEntity>
{
    public TEntity Entity;

    public bool IsSucessful;

    public ResponseObj ErrorResponse;
}
public class ResponseObj
{
    public string FriendlyMessage;
    public string ErrorMessage;
    public string TechnicalMessage;
}