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

            // WHAT: This is a bit of a hack, but it is necessary to hash the password before checking
            //  it against the database.
            // WHY: The reason is that the password is stored in the database as a hash, and we need
            //  to hash the password before we can compare it. However, we don't want to hash the
            //  password if the user doesn't exist, because that would be a waste of resources. So
            //  we first check if the user exists, and if they do, we hash the password and compare
            //  it. If they don't exist, we just return null.
            string hashedPassword = PasswordUtilities.HashPassword(request.Username, request.Password);

            Laborer? entity = await this._dbCtx.Laborers.FirstOrDefaultAsync(e => e.Username.ToLower() == request.Username.ToLower(), cancellationToken);

            if(entity != null)
            {
                if(PasswordUtilities.PasswordsMatch(entity.Password, entity.Username, request.Password))
                {
                    result = (entity != null) ? entity.MapEntityToDomain() : null;
                    result?.TakeSnapshot();
                }
            }

            return result;
        }

        public async Task<Models.Laborer?> Handle(GetLaborerByID request, CancellationToken cancellationToken)
        {
            var entity = await this._dbCtx.Laborers.FirstOrDefaultAsync(e => e.Id == request.LaborerID);

            Models.Laborer? result = (entity != null) ? entity.MapEntityToDomain() : null;
            result?.TakeSnapshot();
            return result;
        }

        #endregion handlers
    }

    public record GetFilteredLaborers(Expression<Func<DataAccess.Entities.Laborer, bool>>? Filter = null) : IRequest<List<Models.Laborer>>;

    public record GetLaborerByCredentials(string Username, string Password) : IRequest<Models.Laborer?>;

    public record GetLaborerByID(int LaborerID) : IRequest<Models.Laborer?>;

}
