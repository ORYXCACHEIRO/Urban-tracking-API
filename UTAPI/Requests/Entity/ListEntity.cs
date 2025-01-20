
namespace UTAPI.Requests.Entity
{
    public class ListEntity
    {
        public Guid Id { get; set; } // Identificador único da entidade

        public string Name { get; set; } // Nome da entidade

        public string Email { get; set; } // E-mail da entidade

        public string Phone { get; set; } // Telefone da entidade

        public bool Active { get; set; } // Se a entidade está ativa ou não

        public string About { get; set; } // Descrição sobre a entidade

        public string WorkHours { get; set; } // Horário de funcionamento da entidade

        public Guid RegionId { get; set; } // Identificador da região associada

        public string RegionName { get; set; } // Nome da região (para retornar o nome da região, caso necessário)
    }
}
