namespace depiR5gp.Repositories.Interfaces;

//Transaction manager & repository aggregator
public interface IUnitOfWork : IDisposable
{
    IChefRepository Chefs { get; }
    IDishRepository Dishes { get; }
    ICartRepository Carts { get; }
    IOrderRepository Orders { get; }
    IReviewRepository Reviews { get; }
    IConversationRepository Conversations { get; }
    ISupportRepository SupportCases { get; }

    Task<int> CompleteAsync();
}

