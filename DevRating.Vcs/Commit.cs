namespace DevRating.Vcs
{
    public interface Commit
    {
        string Sha();
        string Repository();
        string Author();
    }
}