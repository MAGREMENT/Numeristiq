namespace Global;

public delegate T GetArgument<out T>();
public delegate void SetArgument<in T>(T value);