namespace LightningBug.Polly.Providers.Attributes.Scope
{
    public enum Scopes
    {
        Global,
        OnePerInterfaceType,
        OnePerConcreteType,
        OnePerMethod,
        Custom
    }
}