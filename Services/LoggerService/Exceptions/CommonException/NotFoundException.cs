namespace Services.LoggerService.Exceptions.CommonException
{
    public abstract class NotFoundException : Exception
    {
        protected NotFoundException(string message) : base(message) { }
    }
}
