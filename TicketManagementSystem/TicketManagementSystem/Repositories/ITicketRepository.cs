using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TicketManagementSystem.Models;

namespace TicketManagementSystem.Repositories
{
  public interface ITicketRepository
  {
    int CreateTicket(Ticket ticket);
    void UpdateTicket(Ticket ticket);
    Ticket GetTicket(int id);
  }
}
