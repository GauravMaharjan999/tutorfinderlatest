namespace Kachuwa.Dash.Filters
{
    public interface IFilter
    {
        bool RunIndependently { get; set; }
        string Execute();
    }
}