using depiR5gp.Data;
using depiR5gp.Repositories.Interfaces;

namespace depiR5gp.Repositories.Implementations;

// Coordinates transactions across repositories
public class UnitOfWork : IUnitOfWork
{
    private readonly ApplicationDbContext _context;

    public IChefRepository Chefs { get; }
    public IDishRepository Dishes { get; }
    public ICartRepository Carts { get; }
    public IOrderRepository Orders { get; }
    public IReviewRepository Reviews { get; }
    public IConversationRepository Conversations { get; }
    public ISupportRepository SupportCases { get; }

    public UnitOfWork(ApplicationDbContext context)
    {
        _context = context;
        Chefs = new ChefRepository(_context);
        Dishes = new DishRepository(_context);
        Carts = new CartRepository(_context);
        Orders = new OrderRepository(_context);
        Reviews = new ReviewRepository(_context);
        Conversations = new ConversationRepository(_context);
        SupportCases = new SupportRepository(_context);
    }

    // الدالة المطلوبة للواجهة IUnitOfWork
    public async Task<int> SaveChangesAsync()
    {
        return await _context.SaveChangesAsync();
    }

    public async Task<int> CompleteAsync()
    {
        return await SaveChangesAsync();
    }

    public void Dispose()
    {
        _context.Dispose();
        GC.SuppressFinalize(this);
    }

    Task IUnitOfWork.SaveChangesAsync()
    {
        return SaveChangesAsync();
    }
}