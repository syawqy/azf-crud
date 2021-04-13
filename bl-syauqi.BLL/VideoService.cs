using bl_syauqi.DAL.Models;
using Nexus.Base.CosmosDBRepository;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace bl_syauqi.BLL
{
    public class VideoService
    {
        private readonly IDocumentDBRepository<ResourceVideo> _repository;
        public VideoService(IDocumentDBRepository<ResourceVideo> repository)
        {
            if (_repository == null)
            {
                _repository = repository;
            }
        }
        public async Task<ResourceVideo> GetVideoById(string id)
        {
            return await _repository.GetByIdAsync(id);
        }
        public async Task<PageResult<ResourceVideo>> GetVideo()
        {
            var data = await _repository.GetAsync(p => true);
            return data;
        }
        public async Task<ResourceVideo> CreatePerson(ResourceVideo video)
        {
            return await _repository.CreateAsync(video);
        }
        public async Task<ResourceVideo> UpdateVideo(ResourceVideo video)
        {
            return await _repository.UpdateAsync(video.Id, video);
        }
        public async Task<string> DeleteVideo(string id, Dictionary<string, string> pk)
        {
            try
            {
                await _repository.DeleteAsync(id, pk);
                return "Data berhasil dihapus";
            }
            catch
            {
                return "Data tidak ditemukan";
            }

        }
    }
}
