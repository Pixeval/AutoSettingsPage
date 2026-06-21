namespace AutoSettingsPage.Models;

public sealed class SettingsEntryControlFactoryDictionary<TControl>
{
    private readonly Dictionary<Type, Func<ISettingsEntry, TControl>> _exactFactories = [];

    private readonly List<FactoryRegistration> _fallbackFactories = [];

    public Func<ISettingsEntry, TControl> this[Type entryType] => GetFactory(entryType);

    public SettingsEntryControlFactoryDictionary<TControl> Add<TEntry>(Func<TEntry, TControl> factory)
        where TEntry : ISettingsEntry =>
        Add(typeof(TEntry), entry => factory((TEntry) entry));

    public SettingsEntryControlFactoryDictionary<TControl> Add(Type entryType, Func<ISettingsEntry, TControl> factory)
    {
        if (entryType is { IsGenericTypeDefinition: true, IsInterface: true })
            throw new ArgumentException($"Open generic interface registrations are not supported. Use a closed interface with {nameof(AddAssignable)} instead.", nameof(entryType));

        if (entryType.IsInterface || entryType.IsAbstract || entryType.ContainsGenericParameters)
            _fallbackFactories.Add(new(entryType, factory, entryType.IsGenericTypeDefinition
                ? FactoryMatchKind.OpenGeneric
                : FactoryMatchKind.Assignable));
        else
            _exactFactories[entryType] = factory;

        return this;
    }

    public SettingsEntryControlFactoryDictionary<TControl> AddOpenGeneric(Type openGenericEntryType, Func<ISettingsEntry, TControl> factory)
    {
        if (!openGenericEntryType.IsGenericTypeDefinition)
            throw new ArgumentException("The entry type must be an open generic type definition.", nameof(openGenericEntryType));
        if (openGenericEntryType.IsInterface)
            throw new ArgumentException($"Open generic interface registrations are not supported. Use a closed interface with {nameof(AddAssignable)} instead.", nameof(openGenericEntryType));

        _fallbackFactories.Add(new(openGenericEntryType, factory, FactoryMatchKind.OpenGeneric));
        return this;
    }

    public SettingsEntryControlFactoryDictionary<TControl> AddAssignable(Type entryType, Func<ISettingsEntry, TControl> factory)
    {
        if (entryType.ContainsGenericParameters)
            throw new ArgumentException("The entry type must be closed when registered as an assignable type.", nameof(entryType));

        _fallbackFactories.Add(new(entryType, factory, FactoryMatchKind.Assignable));
        return this;
    }

    public SettingsEntryControlFactoryDictionary<TControl> AddAssignable<TEntry>(Func<TEntry, TControl> factory)
        where TEntry : ISettingsEntry =>
        AddAssignable(typeof(TEntry), entry => factory((TEntry) entry));

    public bool TryGetFactory(Type entryType, out Func<ISettingsEntry, TControl> factory)
    {
        if (_exactFactories.TryGetValue(entryType, out factory!))
            return true;

        FactoryRegistration? bestRegistration = null;
        var bestScore = int.MaxValue;
        foreach (var registration in _fallbackFactories)
        {
            if (!registration.TryGetMatchScore(entryType, out var score) || score >= bestScore)
                continue;
            bestRegistration = registration;
            bestScore = score;
        }

        if (bestRegistration is not { } match)
            return false;

        factory = match.Factory;
        return true;
    }

    public Func<ISettingsEntry, TControl> GetFactory(Type entryType) =>
        TryGetFactory(entryType, out var factory)
            ? factory
            : throw new KeyNotFoundException($"No settings entry control factory is registered for '{entryType}'.");

    private readonly record struct FactoryRegistration(
        Type EntryType,
        Func<ISettingsEntry, TControl> Factory,
        FactoryMatchKind MatchKind)
    {
        public bool TryGetMatchScore(Type entryType, out int score)
        {
            score = MatchKind switch
            {
                FactoryMatchKind.Assignable => GetAssignableMatchScore(entryType),
                FactoryMatchKind.OpenGeneric => GetOpenGenericMatchScore(entryType),
                _ => throw new ArgumentOutOfRangeException(nameof(MatchKind))
            };
            return score is not int.MaxValue;
        }

        private int GetAssignableMatchScore(Type entryType) =>
            EntryType.IsAssignableFrom(entryType) ? GetInheritanceDistance(entryType, EntryType) : int.MaxValue;

        private int GetOpenGenericMatchScore(Type? entryType)
        {
            var distance = 0;
            while (entryType is not null && entryType != typeof(object))
            {
                if (entryType.IsGenericType && entryType.GetGenericTypeDefinition() == EntryType)
                    return distance;

                entryType = entryType.BaseType;
                distance++;
            }

            return int.MaxValue;
        }

        private static int GetInheritanceDistance(Type entryType, Type registeredType)
        {
            if (entryType == registeredType)
                return 0;

            var distance = 0;
            for (var type = entryType; type is not null && type != typeof(object); type = type.BaseType)
            {
                if (type == registeredType)
                    return distance;
                distance++;
            }

            return registeredType.IsInterface && registeredType.IsAssignableFrom(entryType)
                ? InterfaceMatchOffset
                : int.MaxValue;
        }

        private const int InterfaceMatchOffset = 1000;
    }

    private enum FactoryMatchKind
    {
        Assignable,
        OpenGeneric
    }
}
