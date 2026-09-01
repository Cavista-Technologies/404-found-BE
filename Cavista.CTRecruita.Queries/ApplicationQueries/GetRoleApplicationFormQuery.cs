using Cavista.CTRecruita.Data.Contexts;
using Cavista.CTRecruita.Data.Entities.Enums;
using Cavista.CTRecruita.Utilities.ApiResponse;
using Cavista.CTRecruita.Utilities.Mediator.Contracts;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Cavista.CTRecruita.Queries.ApplicationQueries
{
    public class GetRoleApplicationFormQuery : IRequest<ApiResponse>
    {
        public string Slug { get; set; }
    }
    public class GetRoleApplicationFormHandler : IRequestHandler<GetRoleApplicationFormQuery, ApiResponse>
    {
        private readonly ApplicationReadOnlyContext _context;
        public GetRoleApplicationFormHandler(ApplicationReadOnlyContext context)
        {
            _context = context;
        }
        public async Task<ApiResponse> Handle(GetRoleApplicationFormQuery request, CancellationToken cancellationToken)
        {
            var form = await _context.ApplicationForms
                .Include(f => f.Fields)
                .FirstOrDefaultAsync(f => f.Slug == request.Slug && f.Status == FormStatus.Published, cancellationToken);
            if (form is null)
                return new ApiResponse(true, (int)StatusCodes.Status404NotFound, "Form not found or not published");
            var result = new
            {
                form.Title,
                form.IntroMessage,
                Fields = form.Fields.OrderBy(x => x.SortOrder).Select(x => new
                {
                    x.Id,
                    x.Label,
                    x.Placeholder,
                    x.FieldType,
                    x.IsRequired,
                    x.SortOrder,
                    Options = x.OptionsJson != null ? JsonSerializer.Deserialize<List<string>>(x.OptionsJson) : null
                })
            };
            return new ApiResponse(false, (int)StatusCodes.Status200OK, "Form retrieved", result);
        }
    }
}
