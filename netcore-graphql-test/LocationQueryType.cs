using HotChocolate.Types;

namespace netcore_graphql_test
{
    public class LocationQueryType : ObjectType<LocationQueries>
    {
        protected override void Configure(IObjectTypeDescriptor<LocationQueries> descriptor)
        {
            base.Configure(descriptor);

            descriptor.Field(x => x.GetLocations(default));

            descriptor.Field(x => x.GetLocation(default, default))
                .Argument("code", a => a.Type<StringType>());
        }
    }
}