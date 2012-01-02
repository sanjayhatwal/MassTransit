﻿// Copyright 2007-2011 Chris Patterson, Dru Sellers, Travis Smith, et. al.
//  
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use 
// this file except in compliance with the License. You may obtain a copy of the 
// License at 
// 
//     http://www.apache.org/licenses/LICENSE-2.0 
// 
// Unless required by applicable law or agreed to in writing, software distributed 
// under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR 
// CONDITIONS OF ANY KIND, either express or implied. See the License for the 
// specific language governing permissions and limitations under the License.
namespace MassTransit.Transports.ZeroMq.Tests
{
    using System;
    using System.Threading;
    using Context;
    using Magnum.Extensions;
    using NUnit.Framework;
    using ZMQ;
    using Magnum.TestFramework;

    [TestFixture]
    public class Inbound_Specs
    {
        InboundZeroMqTransport _inbound;
        Context _context;
        OutboundZeroMqTransport _outbound;

        [SetUp]
        public void SetUp()
        {
            _context = new Context();
            var address  = new ZeroMqAddress(new Uri("zeromq-tcp://localhost:5555"));
            var zeroMqConnection = new ZeroMqConnection(_context, address, SocketType.REQ );
            ConnectionHandler<ZeroMqConnection> connection = new ConnectionHandlerImpl<ZeroMqConnection>(zeroMqConnection);
            _inbound = new InboundZeroMqTransport(address, connection, true);
            _outbound = new OutboundZeroMqTransport(address, connection);

            //push simple message in
            ISendContext sendContext = new SendContext<string>("dru");
            _outbound.Send(sendContext);
        }

        [TearDown]
        public void TearDown()
        {
            _outbound.Dispose();
            _inbound.Dispose();
            _context.Dispose();
        }

        [Test]
        public void SmokeTest()
        {
        }

        [Test]
        public void CanRcv()
        {
            var mre = new ManualResetEvent(false);
            _inbound.Receive(cxt=>
                {
                    return context =>
                        {
                            var x = context.BodyStream.ReadToEndAsText();
                            x.ShouldEqual("dru");
                            mre.Set();
                        };
                }, 20.Seconds());
            mre.WaitOne(5.Seconds());
        }
    }
}