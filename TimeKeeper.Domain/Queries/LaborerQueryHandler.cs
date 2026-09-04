using System.Linq.Expressions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TimeKeeper.DataAccess.Entities;
using TimeKeeper.Domain.Security;
using TimeKeeper.Domain.Utilities.Mappers;

namespace TimeKeeper.Domain.Queries
{
    public class LaborerQueryHandler(TimeKeeperDbContext dbCtx) : 
                                    IRequestHandler<GetLaborerByCredentials, Models.Laborer?>,
                                    IRequestHandler<GetLaborerByID, Models.Laborer?>,
                                    IRequestHandler<GetFilteredLaborers, List<Models.Laborer>>
    {
        #region properties

        private readonly TimeKeeperDbContext _dbCtx = dbCtx;

        #endregion properties

        #region ctor
        #endregion ctor

        #region handlers

        public async Task<List<Models.Laborer>> Handle(GetFilteredLaborers request, CancellationToken cancellationToken)
        {
            List<DataAccess.Entities.Laborer> entities = (request.Filter != null) ? 
                                                            await this._dbCtx.Laborers.Where(request.Filter).ToListAsync(cancellationToken) :
                                                            await this._dbCtx.Laborers.ToListAsync(cancellationToken);

            List<Models.Laborer> results = new List<Models.Laborer>();
            foreach(var entity in entities)
            {
                Models.Laborer model = entity.MapEntityToDomain();
                model.TakeSnapshot();
                results.Add(model);
            }

            return results;
        }

        public async Task<Models.Laborer?> Handle(GetLaborerByCredentials request, CancellationToken cancellationToken)
        {

            Models.Laborer? result = null;

            // Hash the password to compare it to the password in the database
            //
            string hashedPassword = PasswordUtilities.HashPassword(request.Username, request.Password);

            var entity = await this._dbCtx.Laborers.FirstOrDefaultAsync(e => e.Username.ToLower() == request.Username.ToLower());

            if(entity != null)
            {
                if(PasswordUtilities.PasswordsMatch(entity.Password, entity.Username, request.Password))
                {
                    result = (entity != null) ? entity.MapEntityToDomain() : null;
                    result.TakeSnapshot();
                }
            }

            return result;
        }

        public async Task<Models.Laborer?> Handle(GetLaborerByID request, CancellationToken cancellationToken)
        {
            var entity = await this._dbCtx.Laborers.FirstOrDefaultAsync(e => e.Id == request.LaborerID);

            Models.Laborer? result = (entity != null) ? entity.MapEntityToDomain() : null;
            result.TakeSnapshot();
            return result;
        }

        #endregion handlers
    }

    public record GetFilteredLaborers(Expression<Func<DataAccess.Entities.Laborer, bool>>? Filter = null) : IRequest<List<Models.Laborer>>;

    public record GetLaborerByCredentials(string Username, string Password) : IRequest<Models.Laborer?>;

    public record GetLaborerByID(int LaborerID) : IRequest<Models.Laborer?>;

}
