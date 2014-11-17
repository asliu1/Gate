using System.Collections.Generic;
using System.Text.RegularExpressions;

public class PlacableFactory{
    /*
     * The idea behind this architecture is that placeables and components are all predefined.
     * Prefabs only provide the structure and default values for placeables.
     * All the actual data is stored in the placeables Dictionary.
     * The factory does it's own type and error checking with these properties which are stored as 
     * strings so that we can create custom objects when we export to JSON.
     */
    private Dictionary<string,Dictionary<string,List<string>>> placeables;
    private List<Placeable> placeablePrefabs;
    private List<Component> componentPrefabs;
    public PlacableFactory()
    {
        placeables = new Dictionary<string, Dictionary<string,List<string>>>();
        placeablePrefabs = new List<Placeable>();
        componentPrefabs = new List<Component>();
    }
    // Not yet implemented.  Commands are processed through here.
    public string message(string message)
    {
        return "";
    }
    private bool makePlaceablePrefab(string name)
    {
        foreach(Placeable p in placeablePrefabs)
            if(p.getName() == name)
                return false;
        placeablePrefabs.Add(new Placeable(name));
        return true;
    }
    private bool makeComponentPrefab(string name)
    {
        foreach (Component c in componentPrefabs)
            if (c.getName() == name)
                return false;
        componentPrefabs.Add(new Component(name));
        return true;
    }
    private bool makePlaceable(string name, string prefab)
    {
        Dictionary<string, List<string>> components = new Dictionary<string, List<string>>();
        Placeable temp = null;
        foreach (Placeable p in placeablePrefabs)
            if (p.getName() == name)
            {
                temp = p;
                break;
            }
        if (temp == null)
            return false;
        foreach (Component c in temp.getComponents())
        {
            List<string> propertyValues = new List<string>();
            foreach (Property pr in c.getProperties())
            {
                propertyValues.Add(pr.getValue());
            }
            components.Add(c.getName(), propertyValues);
        }
        placeables.Add(name, components);
        return true;
    }
    private class Property
    {
        string name;
        byte type;
        string value;
        public Property(string name, byte type)
        {
            this.name = name;
            this.type = type;
            this.value =
                type == 0 ? "false" :
                type == 1 ? "0" :
                type == 2 ? "" :
                "__error__";
        }
        public void setValue(string value)
        {
            this.value =
                this.type == 0 ?
                    (value == "false" || value == "true") ?
                        value : "__error__" :
                this.type == 1 ?
                    Regex.IsMatch(value, @"^\d+$") ?
                        value : "__error__" :
                this.type == 2 ?
                    value :
                "__error__";
        }
        public string getName()
        {
            return this.name;
        }
        public string getType()
        {
            return
                this.type == 0 ? "bool" :
                this.type == 1 ? "int" :
                this.type == 2 ? "string" :
                "__error__";
        }
        public string getValue()
        {
            return this.value;
        }
    }
    private class Component
    {
        string name;
        List<Property> properties;
        public Component(string name)
        {
            this.name = name;
            properties = new List<Property>();
        }
        public string getName()
        {
            return this.name;
        }
        public bool addProperty(string name, byte type)
        {
            Property temp = new Property(name, type);
            if (temp.getValue() == "__error__" || hasProperty(name))
                return false;
            properties.Add(temp);
            return true;
        }
        public bool removeProperty(string name)
        {
            if (hasProperty(name))
                for (int i = 0; i < properties.Count; i++)
                    if (properties[i].getName() == name)
                    {
                        properties.RemoveAt(i);
                        return true;
                    }
            return false;
        }
        public bool hasProperty(string name)
        {
            for (int i = 0; i < properties.Count; i++)
                if (properties[i].getName() == name)
                    return true;
            return false;
        }
        public List<Property> getProperties()
        {
            return this.properties;
        }
    }
    private class Placeable
    {
        int id;
        string name;
        List<Component> components;
        public Placeable(string name)
        {
            this.name = name;
            components = new List<Component>();
        }
        public bool addComponent(Component c)
        {
            components.Add(c);
            return true;
        }
        public int getId()
        {
            return this.id;
        }
        public string getName()
        {
            return this.name;
        }
        public List<Component> getComponents()
        {
            return this.components;
        }
    }
}
