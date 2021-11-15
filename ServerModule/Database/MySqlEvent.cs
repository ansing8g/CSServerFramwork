namespace ServerModule.Database
{
    public interface MySqlEvent
    {
        public void OnError(string _error_type, System.Exception _exception);
        public void OnError(string _error_type, IMySqlExecutor _executor, System.Exception _exception);
    }
}
