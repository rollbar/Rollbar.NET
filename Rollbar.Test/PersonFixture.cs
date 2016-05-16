using Newtonsoft.Json;
using Xunit;

namespace Rollbar.Test {
    public class PersonFixture {
        [Fact]
        public void Person_id_rendered_correctly() {
            var rp = new Person("person_id");
            Assert.Equal("{\"id\":\"person_id\"}", JsonConvert.SerializeObject(rp));
        }

        [Fact]
        public void Person_username_rendered_correctly() {
            var rp = new Person("person_id") {
                UserName = "chris_pfohl",
            };
            Assert.Equal("{\"id\":\"person_id\",\"username\":\"chris_pfohl\"}", JsonConvert.SerializeObject(rp));
        }

        [Fact]
        public void Person_email_rendered_correctly() {
            var rp = new Person("person_id") {
                Email = "chris@rollbar.com",
            };
            Assert.Equal("{\"id\":\"person_id\",\"email\":\"chris@rollbar.com\"}", JsonConvert.SerializeObject(rp));
        }
    }
}
