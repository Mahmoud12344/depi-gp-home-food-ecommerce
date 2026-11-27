using depiR5gp.Models.Payments;
using depiR5gp.Models.Support;
using depiR5gp.Repositories.Implementations;

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
    IUserRepository Users { get; }
    IPlatformSettingRepository PlatformSettingRepository { get; }
    IGenericRepository<Faq> Faqs { get; }
    IGenericRepository<Payout> Payouts { get; }
    IGenericRepository<LedgerEntry> LedgerEntries { get; }

    Task<int> CompleteAsync();
    Task SaveChangesAsync();
}

