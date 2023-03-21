using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TicketManagementSystem.Models;

namespace TicketManagementSystem.Repositories
{
  internal class TicketRepositoryProxy : ITicketRepository
  {
    public int CreateTicket(Ticket ticket)
      => TicketRepository.CreateTicket(ticket);

    public Ticket GetTicket(int id)
      => TicketRepository.GetTicket(id);

    public void UpdateTicket(Ticket ticket)
    {
      TicketRepository.UpdateTicket(ticket);
    }
  }
}
