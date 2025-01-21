namespace MarcSync;

public class MarcSyncClient
{
    private readonly string accessToken;

    public MarcSyncClient(string accessToken)
    {
        this.accessToken = accessToken;
    }

    public Collection GetCollection(string collectionName)
    {
        return new Collection(accessToken, collectionName);
    }
}