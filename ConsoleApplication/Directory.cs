using Model.Utility.Collections.Lexicography;

namespace ConsoleApplication;

public class Directory
{
    private readonly List<Command> _commands = new();
    private readonly List<Directory> _directories = new();
    private readonly Trie<object> _trie = new ListTrie<object>();
    
    private int _defaultCommand = -1;
    private readonly string[] _synonyms;
    
    public IReadOnlyList<Command> Commands => _commands;

    public IReadOnlyList<Directory> Directories => _directories;
    
    public string Name { get; }
    
    public Directory(string name, params string[] synonyms)
    {
        Name = name;
        _synonyms = synonyms;
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

        if(!_trie.Add(command.Name, command)) throw new ArgumentException("Already exists");
        foreach (var s in command.Synonyms)
        {
            if(!_trie.Add(s, command)) throw new ArgumentException("Already exists");
        }

        return this;
    }

    public Directory AddDirectory(Directory directory)
    {
        _directories.Add(directory);
        
        if(!_trie.Add(directory.Name, directory)) throw new ArgumentException("Already exists");
        foreach (var s in directory._synonyms)
        {
            if(!_trie.Add(s, directory)) throw new ArgumentException("Already exists");
        }

        return directory;
    }

    public Directory? FindDirectory(string name)
    {
        return _trie.TryGet(name, out var obj) && obj is Directory dir ? dir : null;
    }

    public Command? FindCommand(string name)
    {
        return _trie.TryGet(name, out var obj) && obj is Command cmd ? cmd : null;
    }

    public Command? DefaultCommand()
    {
        if (_defaultCommand < 0) return null;

        return _commands[_defaultCommand];
    }
}