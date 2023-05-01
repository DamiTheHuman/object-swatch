public class SwatchData<T>
{
    public T genericObject;
    public string parentDirectory = "";

    public SwatchData() { }

    public SwatchData(T genericObject, string parentDirectory)
    {
        this.genericObject = genericObject;
        this.parentDirectory = parentDirectory;
    }

}
