Informação importante para o desenvolvimento do projeto:

Para identificar erros que possam existir no projeto correr `dotnet build` no cmd (root do projeto);

Sempre que for para dar migrate na BD correr o comando `Add-Migration` com o nome `v1.xxxx`. Por fim correr o comando `Update-Database`;

Sempre que se cria um serviço cria-se tb uma interface;

Sempre que se cria um serviço, e uma interface para o mesmo, por favor seguir o padrão de injeção de serviços já establecido no ficheiro `Program.cs`;


Criar controlador api,
Models crias a class segue os padroes do outro ja criado,
Criar Interface,
Criar Serviço,
Programa.cs injetar o serviço, (head sculpt)
Pasta Requests, criar pasta "nome da tabela", criar ficheiros do request .class