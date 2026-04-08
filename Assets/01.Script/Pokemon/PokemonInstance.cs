public class PokemonInstance
{
    public PokemonData rootData;
    public PokemonData data;

    public int enhanceLevel;

    public Gender gender;
    public bool isShiny;

    public int formIndex;

    public PokemonInstance(PokemonData data, Gender gender, bool isShiny, int formIndex = 0)
    {
        this.rootData = data;
        this.data = data;
        this.gender = gender;
        this.isShiny = isShiny;
        this.formIndex = formIndex;
        this.enhanceLevel = 0;
    }
}