namespace NLightning.Persistence
{
    public class PersistenceService : IPersistenceService
    {
        private readonly LocalPersistenceContext _localPersistenceContext;
        private readonly NetworkPersistenceContext _networkPersistenceContext;

        public PersistenceService(LocalPersistenceContext localPersistenceContext, NetworkPersistenceContext networkPersistenceContext)
        {
            _localPersistenceContext = localPersistenceContext;
            _networkPersistenceContext = networkPersistenceContext;
        }
        
        public void Initialize()
        {
            _localPersistenceContext.Database.EnsureCreated();
            _networkPersistenceContext.Database.EnsureCreated();
        }
    }
}