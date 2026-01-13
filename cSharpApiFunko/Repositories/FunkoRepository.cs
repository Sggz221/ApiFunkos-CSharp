using cSharpApiFunko.Models;
using NLog;

namespace cSharpApiFunko.Repositories;

public class FunkoRepository : IFunkoRepository
{
    private static Logger log = LogManager.GetCurrentClassLogger();
    private static long _nextId = 1;

    private Dictionary<long, Funko> _funkos = new Dictionary<long, Funko>()
    {
        { GetNextId(), new Funko() {  Id = _nextId, Nombre = "Pepito", Categoria = "PERSONA", Precio = 9.99 } },
        { GetNextId(), new Funko() {  Id = _nextId, Nombre = "Sullyvan", Categoria = "MONSTRUO", Precio = 19.99 } },
        { GetNextId(), new Funko() {  Id = _nextId, Nombre = "Caca", Categoria = "COSA", Precio = 6.99 } },
        { GetNextId(), new Funko() {  Id = _nextId, Nombre = "Botella", Categoria = "COSA", Precio = 999.99 } },
    };
    
    public async Task<Funko?> GetByIdAsync(int id) 
    {
        log.Debug($"Buscando funko con id: {id}");
        return await Task.FromResult(_funkos[id]);
    }

    public async Task<List<Funko>> GetAllAsync()
    {
        log.Debug("Obteniendo todos los funkos...");
        return await Task.FromResult(_funkos.Values.ToList());
    }

    public async Task<Funko> SaveAsync(Funko item)
    {
        item.Id = GetNextId();
        log.Debug($"Guardando funko {item}");
        _funkos.Add(item.Id, item);
        return await Task.FromResult(_funkos[item.Id]);
    }

    public async Task<Funko?> UpdateAsync(long id, Funko item)
    {
        log.Debug($"Guardando funko con id {item}");
        if (!_funkos.ContainsKey(id))
        {
            log.Error("");
            return await Task.FromResult<Funko?>(null);
        }
        item.UpdatedAt = DateTime.Now;
        item.Id = id;
        _funkos.Add(id, item);
        return await Task.FromResult(item);
    }

    public Task<Funko?> DeleteAsync(Funko item)
    {
        throw new NotImplementedException();
    }

    private static long GetNextId()
    {
        return ++_nextId;
    }
}