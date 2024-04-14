namespace ConsoleApplication;

public class Directory
{
    private readonly List<Command> _commands = new();
    private readonly List<Directory> _directories = new();
    private int _defaultCommand = -1;
    
    public IReadOnlyList<Command> Commands => _commands;

    public IReadOnlyList<Directory> Directories => _directories;
    
    public string Name { get; }
    
    public Directory(string name)
    {
        Name = name;
    }
    
    public Directory AddCommand(Command command, bool isDefault = false)
    {
        if (isDefault)
        {
            if (command.Arguments.Count > 0)
                throw new ArgumentException("A default command cannot have arguments", nameof(isDefault));
            _defaultCommand = _commands.Count;
        }
        _commands.Add(command);

        return this;
    }

    public Directory AddDirectory(Directory directory)
    {
        _directories.Add(directory);

        return directory;
    }

    public Directory? FindDirectory(string name)
    {
        foreach (var directory in _directories)
        {
            if (directory.Name == name) return directory;
        }

        return null;
    }

    public Command? FindCommand(string name)
    {
        foreach (var command in _commands)
        {
            if (command.Name == name) return command;
        }

        return null;
    }

    public Command? DefaultCommand()
    {
        if (_defaultCommand < 0) return null;

        return _commands[_defaultCommand];
    }
}