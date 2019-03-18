using Microsoft.VisualStudio.TestTools.UnitTesting;
using KoperasiDataSyncronization;
using System;

namespace KoperasiDataSyncUnitTest
{
    [TestClass]
    public class KoperasiDataSyncTest
    {
        [TestMethod]
        public void EncryptTest()
        {
            DataEncryption dataEncrypt = new DataEncryption();
            dataEncrypt.secret_key = "(IV8FxF!cv~JT3)v+iVRb/Kr@{kipW";
            dataEncrypt.secret_iv = "apasajakalauyangini";
            String result = dataEncrypt.Encrypt("suwandi");
            String expected = "ZGIvVisvQmQzNU1xVkJMcmh1V0dzZz09";
            Assert.AreEqual(expected, result);
        }

        [TestMethod]
        public void DecryptTest()
        {
            DataEncryption dataEncrypt = new DataEncryption();
            dataEncrypt.secret_key = "(IV8FxF!cv~JT3)v+iVRb/Kr@{kipW";
            dataEncrypt.secret_iv = "apasajakalauyangini";
            String result = dataEncrypt.Decrypt("ZGIvVisvQmQzNU1xVkJMcmh1V0dzZz09");
            String expected = "suwandi";
            Assert.AreEqual(expected, result);
        }
    }
}
