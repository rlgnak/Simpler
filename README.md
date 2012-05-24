#Simpler

You probably won't like Simpler.  If you enjoy spending your time configuring ORMs, interfacing with DI/IOC frameworks, generating code, and building complex domain models, then you will probably hate Simpler.  Simpler's primary goal is help developers create quick, simple solutions while writing the least amount of code possible.  Every piece of code in an application should have a clearly visible business purpose - the rest is just noise.

---

###"What is it?"

For the most part, Simpler is just a philosophy on .NET class design.  All classes that contain functionality are defined as Tasks, named as verbs.  A Task has optional input and/or outputs (POCOs), along with a single Execute() method - and that's it.  Simpler comes with a Task base class, a static TaskFactory class for instantiating Tasks, along with various built-in Tasks that you can use as sub-tasks in your Tasks (see the second example).

	'
    class Ask : Task
    {
        // Inputs
        public string Question { get; set; }

        // Outputs
        public string Answer { get; private set; }

        public override void Execute()
        {
            Answer =
                Question == "Is this cool?"
                ? "Definitely."
                : "Get a life.";
        }
    }
	'

---

###"What's the purpose of the TaskFactory?"

TaskFactory appears to just return an instance of the given Task type, but it actually returns a proxy to the Task. The proxy allows for intercepting Task Execute() calls and performing actions before and/or after the Task execution. For example, the built-in InjectSubTasks attribute will automatically instantiate sub-task properties (only if null) before Task execution, and automatically dispose of them after execution.

	'
    [InjectSubTasks]
    class BeAnnoying : Task
    {
        // Sub-tasks
        public Ask Ask { get; set; }

        public override void Execute()
        {
            const string question = "Is this cool?";

            for (int i = 0; i < 10; i++)
            {
                // Notice that AnswerUsingStaticProperies was injected.
                Ask.Question = question;
                Ask.Execute();
            }
        }
    }

    class Program
    {
        Program()
        {
            var beAnnonying = TaskFactory<BeAnnoying>.Create();
            beAnnonying.Execute();
        }
    }
	'

Sub-task injection simplifies the code, but more importantly it allows for mocking sub-tasks as necessary in Task unit tests.

---

###"What about database interaction?"

Simpler provides a small set of Tasks for interacting with System.Data.IDbCommand. Using SQL, Simpler makes it pretty easy to get data out of a database and into .NET objects, or persist data from .NET objects to a database.

	'
    class SomePoco 
    {
        public bool AmIImportant { get; set; }
    }

    [InjectSubTasks]
    class FetchSomeStuff : Task
    {
        // Inputs
        public string SomeCriteria { get; set; }

        // Outputs
        public SomePoco[] SomePocos { get; set; }

        // Sub-tasks (BuildParametersUsing<T> and FetchListOf<T> are built-in Simpler Tasks)
        public BuildParametersUsing<FetchSomeStuff> BuildParameters { get; set; }
        public FetchListOf<SomePoco> FetchList { get; set; }

        public override void Execute()
        {
            using (var connection = new SqlConnection("MyConnectionString"))
            using (var command = connection.CreateCommand())
            {
                connection.Open();
                command.Connection = connection;
                command.CommandText =
                    @"
                    select 
                        SomeStoredBit as AmIImportant
                    from 
                        ABunchOfJoinedTables
                    where 
                        SomeColumn = @SomeCriteria 
                    ";

                // Use the SomeCriteria property value on this Task to build the @SomeCriteria parameter.
                BuildParameters.CommandWithParameters = command;
                BuildParameters.ObjectWithValues = this;
                BuildParameters.Execute();

                FetchList.SelectCommand = command;
                FetchList.Execute();
                SomePocos = FetchList.ObjectsFetched;
            }
        }
    }
	'

Simpler isn't a full-featured ORM, but it gets the job done.

---

###"You do write tests, don't you?"

	'
    [TestFixture]
    public class FetchSomeStuffTest
    {
        [Test]
        public void should_return_9_pocos()
        {
            // Arrange
            var task = TaskFactory<FetchSomeStuff>.Create();

            // Act
            task.Execute();

            // Assert
            Assert.That(task.SomePocos.Length, Is.EqualTo(9));
        }
    }
	'

By design, all classes clearly define their inputs, outputs, and code to test, so test are real straightforward.

---

Simpler is a tool for developing applications as sets of consistent, discrete, interchangable classes that aren't THINGS, but rather DO THINGS. Simpler works great in team environments because all developers on the team design classes using the same techniques and terminology, resulting in a consistent code base. Simpler fits like a glove with a ASP.NET MVC (add a Tasks folder next to your Controllers, Models, and Views folders). And finally, Simpler can be installed using NuGet.

**Simpler is licensed under the MIT License.  A copy of the MIT license can be found in the LICENSE file.**
