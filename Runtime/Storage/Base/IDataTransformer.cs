namespace CustomUtils.Runtime.Storage.Base
{
    public interface IDataTransformer
    {
        object TransformForStorage(byte[] data);
        byte[] TransformFromStorage(object storedData);
    }
}