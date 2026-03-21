using System.Net.Http;
using System.Net.Http.Json;

namespace AguaMinami.WPF.Services;


public class ApiService
{
    private readonly HttpClient _http;

    public ApiService(string baseUrl = "http://localhost:5000")
    {
        _http = new HttpClient { BaseAddress = new Uri(baseUrl) };
    }

    // GET (listar) 
    public async Task<List<T>> GetAllAsync<T>(string endpoint)
    {
        try
        {
            var result = await _http.GetFromJsonAsync<List<T>>($"api/{endpoint}");
            return result ?? new List<T>();
        }
        catch (Exception ex)
        {
            System.Windows.MessageBox.Show(
                $"Error al conectar con la API: {ex.Message}",
                "Error de conexión", System.Windows.MessageBoxButton.OK,
                System.Windows.MessageBoxImage.Warning);
            return new List<T>();
        }
    }

    // GET (por ID) 
    public async Task<T?> GetByIdAsync<T>(string endpoint, object id)
    {
        try
        {
            return await _http.GetFromJsonAsync<T>($"api/{endpoint}/{id}");
        }
        catch { return default; }
    }

    //  POST (crear) 
    public async Task<bool> CreateAsync<T>(string endpoint, T dto)
    {
        try
        {
            var response = await _http.PostAsJsonAsync($"api/{endpoint}", dto);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            System.Windows.MessageBox.Show($"Error al crear: {ex.Message}", "Error");
            return false;
        }
    }

    //  PUT (actualizar) 
    public async Task<bool> UpdateAsync<T>(string endpoint, object id, T dto)
    {
        try
        {
            var response = await _http.PutAsJsonAsync($"api/{endpoint}/{id}", dto);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            System.Windows.MessageBox.Show($"Error al actualizar: {ex.Message}", "Error");
            return false;
        }
    }

    //  DELETE (eliminar) 
    public async Task<bool> DeleteAsync(string endpoint, object id)
    {
        try
        {
            var response = await _http.DeleteAsync($"api/{endpoint}/{id}");
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            System.Windows.MessageBox.Show($"Error al eliminar: {ex.Message}", "Error");
            return false;
        }
    }
}
