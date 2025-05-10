using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;

namespace BudgetAppBackend.Application.Features.AI.GetAiRecommendations
{
    public class GetAiRecommendationsQuery : IRequest<string>
    {
        public Guid UserId { get; set; }

        public GetAiRecommendationsQuery(Guid userId)
        {
            UserId = userId;
        }
    }
}
