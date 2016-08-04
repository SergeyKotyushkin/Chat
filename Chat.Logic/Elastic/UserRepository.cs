using System;
using Chat.Logic.Elastic.Contracts;
using Chat.Logic.StructureMap;
using Chat.Models;
using Nest;
using User = Chat.Logic.Elastic.Models.User;

namespace Chat.Logic.Elastic
{
    public class UserRepository : IUserRepository
    {
        private const string EsType = "user";

        private readonly IElasticRepository _elasticRepository = StructureMapFactory.Resolve<IElasticRepository>();
        private readonly IEntityRepository _entityRepository = StructureMapFactory.Resolve<IEntityRepository>();


        public ElasticResult<User> Login(string login, string password)
        {
            var searchDescriptor = new SearchDescriptor<User>().Query(
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

        public ElasticResult<User> CheckToken(string token)
        {
            var searchDescriptor = new SearchDescriptor<User>().Query(
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

        public ElasticResult<User> CheckLogin(string login)
        {
            var searchDescriptor = new SearchDescriptor<User>().Query(
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

        public ElasticResult<User> Add(string login, string password)
        {
            var user = new User(login, password);

            return _entityRepository.Add(EsType, user);
        }
    }
}