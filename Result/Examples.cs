namespace Result;

public class Examples
{
    public record Color();

    public Dictionary<string, Color> FavoriteColors = new()
    {
        {"Alice", new Color()},
        {"Bob", new Color()},
    };

    public Color? GetFavoriteColor0(string name)
    {
        if (FavoriteColors.TryGetValue(name, out var color))
        {
            return color;
        }
        return null;
    }

    public Result<Color> GetFavoriteColor1(string name)
    {
        if (FavoriteColors.TryGetValue(name, out var color))
        {
            return Result<Color>.FromOk(color);
        }
        return Result<Color>.FromError(new($"No favorite color for {name}"));
    }

    public Result<Color> GetFavoriteColor(string name)
    {
        if (FavoriteColors.TryGetValue(name, out var color))
        {
            return color;
        }
        return new Error($"No favorite color for {name}");
    }

    public void PrintColor1(string name)
    {
        var colorResult = GetFavoriteColor(name);
        if (colorResult.IsOk)
        {
            Console.WriteLine($"Favorite color for {name} is {colorResult.Value}");
        }
        else
        {
            Console.WriteLine($"{name}s favorite color is unknown");
        }
    }

    public void PrintColor2(string name)
    {
        var colorResult = GetFavoriteColor(name);
        if (colorResult.TryGetValue(out var color))
        {
            Console.WriteLine($"Favorite color for {name} is {color}");
        }
        else
        {
            Console.WriteLine(colorResult.Error);
        }
    }

    public record Postcard();
    Result<Postcard> CreatePostcard(Color background)
    {
        return new Postcard();
    }

    decimal ComputePostage(Postcard postcard) => 0;

    public Result<decimal> CostPerPerson(string name)
    {
        var colorResult = GetFavoriteColor(name);
        if (colorResult.TryGetValue(out var color))
        {
            var postcardResult = CreatePostcard(color);
            if (postcardResult.TryGetValue(out var postcard))
            {
                return ComputePostage(postcard);
            }
            return postcardResult.Error;
        }
        return colorResult.Error;
    }

    public Result<decimal> CostPerPerson2(string name)
    {
        return GetFavoriteColor(name)
            .AndThen(CreatePostcard)
            .Map(ComputePostage);
    }

    public record A; public record B; public record C;
    public record Combined(A A, B B, C C);

    Result<A> MaybeA() => new A();
    Result<B> MaybeB() => new Error("B");
    Result<C> MaybeC() => new C();

    public Result<Combined> Combine()
    {
        var createCombined = (A a, B b, C c) => new Combined(a, b, c);

        // this only works for stuff things that are already delegates
        return createCombined.Lifted(MaybeA(), MaybeB(), MaybeC());

        static Combined CreateCombined(A a, B b, C c) => new(a, b, c);
        return ApplicativeOperations.Lifted(CreateCombined, MaybeA(), MaybeB(), MaybeC());
    }
}
