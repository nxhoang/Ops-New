using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using OPS_DAL;

namespace OPS_DAL.CuttingPlanService
{
	public interface ICutTicketService : CuttingPlanRepository.ICutTicketRepository { }
	public class CutTicketService : CuttingPlanRepository.CutTicketRepository, ICutTicketService{}
}
