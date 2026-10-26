using depiR5gp.Data;
using depiR5gp.Models.Messaging;
using depiR5gp.Repositories.Interfaces;

namespace depiR5gp.Repositories.Implementations;

public class ConversationRepository(ApplicationDbContext context) : GenericRepository<Conversation>(context), IConversationRepository
{
}

