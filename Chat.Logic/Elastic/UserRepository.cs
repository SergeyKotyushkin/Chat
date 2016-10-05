using Chat.Logic.Elastic.Contracts;
using Chat.Logic.Elastic.Models;
using Chat.Logic.StructureMap;
using Chat.Models;
using Nest;

namespace Chat.Logic.Elastic
{
    public class UserRepository : IUserRepository
    {
        private const string EsType = "user";
        private const string UserNameRegexpFormatString = "[a-zA-Z0-9 ]*{0}[a-zA-Z0-9 ]*";

        private readonly IElasticRepository _elasticRepository = StructureMapFactory.Resolve<IElasticRepository>();
        private readonly IEntityRepository _entityRepository = StructureMapFactory.Resolve<IEntityRepository>();


        public ElasticResult<ElasticUser> Login(string login, string password)
        {
            var searchDescriptor = new SearchDescriptor<ElasticUser>().Query(
                q =>
                    q.Bool(
                        b =>
                            b.Must(
                                m =>
                                    m.Term(fields => fields.Field(f => f.Login).Value(login)) &&
                                    m.Term(fields => fields.Field(f => f.Password).Value(password)))))
                .Index(_elasticRepository.EsIndex)
                .Type(EsType);

            var response = _elasticRepository.ExecuteSearchRequest(searchDescriptor);

            return _entityRepository.GetEntityIfOnlyOneEntityInElasticResponse(response);
        }

        public ElasticResult<ElasticUser> CheckToken(string token)
        {
            var searchDescriptor = new SearchDescriptor<ElasticUser>().Query(
                q =>
                    q.Bool(
                        b =>
                            b.Must(
                                m =>
                                    m.Term(fields => fields.Field(f => f.Token).Value(token)))))
                .Index(_elasticRepository.EsIndex)
                .Type(EsType);

            var response = _elasticRepository.ExecuteSearchRequest(searchDescriptor);

            return _entityRepository.GetEntityIfOnlyOneEntityInElasticResponse(response);
        }

        public ElasticResult<ElasticUser> CheckLogin(string login)
        {
            var searchDescriptor = new SearchDescriptor<ElasticUser>().Query(
                q =>
                    q.Bool(
                        b =>
                            b.Must(
                                m =>
                                    m.Term(fields => fields.Field(f => f.Login).Value(login)))))
                .Index(_elasticRepository.EsIndex)
                .Type(EsType);

            var response = _elasticRepository.ExecuteSearchRequest(searchDescriptor);

            return _entityRepository.GetEntityIfOnlyOneEntityInElasticResponse(response);
        }

        public ElasticResult<ElasticUser> Add(string login, string password)
        {
            var user = new ElasticUser(login, password);

            return _entityRepository.Add(EsType, user);
        }

        public ElasticResult<ElasticUser> Update(ElasticUser user)
        {
            var response = _elasticRepository.ExecuteCreateOrUpdateRequest(user, EsType);

            return response.Success
                ? ElasticResult<ElasticUser>.SuccessResult(user)
                : ElasticResult<ElasticUser>.FailResult(response.Message);
        }

        public ElasticResult<ElasticUser[]> GetAll()
        {
            return _entityRepository.GetAll<ElasticUser>(EsType);
        }

        public ElasticResult<ElasticUser> Get(string guid)
        {
            return _entityRepository.Get<ElasticUser>(EsType, guid);
        }

        public ElasticResult<ElasticUser[]> SearchByUserName(string userName)
        {
            var searchDescriptor = new SearchDescriptor<ElasticUser>().Query(
                q =>
                    q.Regexp(
                        r =>
                            r.Field(fields => fields.UserName)
                                .Value(string.Format(UserNameRegexpFormatString, userName))))
                .Index(_elasticRepository.EsIndex)
                .Type(EsType);

            var response = _elasticRepository.ExecuteSearchRequest(searchDescriptor);

            return _entityRepository.GetEntitiesFromElasticResponse(response);
        }

        public ElasticResult<ElasticUser[]> GetAllByGuids(params string[] userGuids)
        {
            return _entityRepository.GetByGuids<ElasticUser>(EsType, userGuids);
        }
    }
}