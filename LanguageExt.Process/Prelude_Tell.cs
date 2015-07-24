﻿using System;
using System.Linq;
using System.Reactive.Linq;
using static LanguageExt.Prelude;
using static LanguageExt.Map;

namespace LanguageExt
{
    /// <summary>
    /// 
    ///     Process:  Tell functions
    /// 
    ///     'Tell' is used to send a message from one process to another (or from outside a process to a process).
    ///     The messages are sent to the process asynchronously and join the process' inbox.  The process will 
    ///     deal with one message from its inbox at a time.  It cannot start the next message until it's finished
    ///     with a previous message.
    /// 
    /// </summary>
    public static partial class Process
    {
        /// <summary>
        /// Send a message to a process
        /// </summary>
        /// <param name="pid">Process ID to send to</param>
        /// <param name="message">Message to send</param>
        /// <param name="sender">Optional sender override.  The sender is handled automatically if you do not provide one.</param>
        public static Unit tell(ProcessId pid, object message, ProcessId sender = default(ProcessId)) =>
            message is SystemMessage
                ? ActorContext.TellSystem(pid, message as SystemMessage)
                : message is UserControlMessage
                    ? ActorContext.TellUserControl(pid, message as UserControlMessage)
                    : ActorContext.Tell(pid, message, sender);

        /// <summary>
        /// Send a message at a specified time in the future
        /// </summary>
        /// <returns>IDisposable that you can use to cancel the operation if necessary.  You do not need to call Dispose 
        /// for any other reason.</returns>
        /// <param name="pid">Process ID to send to</param>
        /// <param name="message">Message to send</param>
        /// <param name="sender">Optional sender override.  The sender is handled automatically if you do not provide one.</param>
        /// <param name="delayFor">How long to delay sending for</param>
        public static IDisposable tell(ProcessId pid, object message, TimeSpan delayFor, ProcessId sender = default(ProcessId)) =>
            delay(() => tell(pid, message, sender), delayFor).Subscribe();

        /// <summary>
        /// Send a message at a specified time in the future
        /// </summary>
        /// <remarks>
        /// This will fail to be accurate across a Daylight Saving Time boundary
        /// </remarks>
        /// <returns>IDisposable that you can use to cancel the operation if necessary.  You do not need to call Dispose 
        /// for any other reason.</returns>
        /// <param name="pid">Process ID to send to</param>
        /// <param name="message">Message to send</param>
        /// <param name="sender">Optional sender override.  The sender is handled automatically if you do not provide one.</param>
        /// <param name="delayUntil">Date and time to send</param>
        public static IDisposable tell(ProcessId pid, object message, DateTime delayUntil, ProcessId sender = default(ProcessId)) =>
            delay(() => tell(pid, message, sender), delayUntil).Subscribe();

        /// <summary>
        /// Tell children the same message
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public static Unit tellChildren(object message) =>
            iter(Children, child => tell(child, message));

        /// <summary>
        /// Tell children the same message, delayed.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="delayFor">How long to delay sending for</param>
        /// <returns>IDisposable that you can use to cancel the operation if necessary.  You do not need to call Dispose 
        /// for any other reason.</returns>
        public static IDisposable tellChildren(object message, TimeSpan delayFor) =>
            delay(() => tellChildren(message), delayFor).Subscribe();

        /// <summary>
        /// Tell children the same message, delayed.
        /// </summary>
        /// <remarks>
        /// This will fail to be accurate across a Daylight Saving Time boundary
        /// </remarks>
        /// <param name="message"></param>
        /// <param name="delayUntil">Date and time to send</param>
        /// <returns>IDisposable that you can use to cancel the operation if necessary.  You do not need to call Dispose 
        /// for any other reason.</returns>
        public static IDisposable tellChildren(object message, DateTime delayUntil) =>
            delay(() => tellChildren(message), delayUntil).Subscribe();

        /// <summary>
        /// Tell children the same message
        /// The list of children to send to are filtered by the predicate provided
        /// </summary>
        /// <param name="message">Message to send</param>
        public static Unit tellChildren(object message, Func<ProcessId,bool> predicate) =>
            iter(filter(Children, predicate), child => tell(child, message));

        /// <summary>
        /// Tell children the same message, delayed.
        /// The list of children to send to are filtered by the predicate provided
        /// </summary>
        /// <param name="message">Message to send</param>
        /// <param name="delayFor">How long to delay sending for</param>
        /// <returns>IDisposable that you can use to cancel the operation if necessary.  You do not need to call Dispose 
        /// for any other reason.</returns>
        public static IDisposable tellChildren(object message, TimeSpan delayFor, Func<ProcessId, bool> predicate) =>
            delay(() => tellChildren(message, predicate), delayFor).Subscribe();

        /// <summary>
        /// Tell children the same message, delayed.
        /// The list of children to send to are filtered by the predicate provided
        /// </summary>
        /// <remarks>
        /// This will fail to be accurate across a Daylight Saving Time boundary
        /// </remarks>
        /// <param name="message">Message to send</param>
        /// <param name="delayUntil">Date and time to send</param>
        /// <returns>IDisposable that you can use to cancel the operation if necessary.  You do not need to call Dispose 
        /// for any other reason.</returns>
        public static IDisposable tellChildren(object message, DateTime delayUntil, Func<ProcessId, bool> predicate) =>
            delay(() => tellChildren(message, predicate), delayUntil).Subscribe();

        /// <summary>
        /// Send a message to the parent process
        /// </summary>
        /// <param name="message">Message to send</param>
        /// <param name="sender">Optional sender override.  The sender is handled automatically if you do not provide one.</param>
        public static Unit tellParent(object message) =>
            tell(ActorContext.Parent, message);

        /// <summary>
        /// Send a message to the parent process at a specified time in the future
        /// </summary>
        /// <returns>IDisposable that you can use to cancel the operation if necessary.  You do not need to call Dispose 
        /// for any other reason.</returns>
        /// <param name="message">Message to send</param>
        /// <param name="sender">Optional sender override.  The sender is handled automatically if you do not provide one.</param>
        /// <param name="delayFor">How long to delay sending for</param>
        public static IDisposable tellParent(object message, TimeSpan delayFor, ProcessId sender = default(ProcessId)) =>
            tell(ActorContext.Parent, message, delayFor);

        /// <summary>
        /// Send a message to the parent process at a specified time in the future
        /// </summary>
        /// <remarks>
        /// This will fail to be accurate across a Daylight Saving Time boundary
        /// </remarks>
        /// <returns>IDisposable that you can use to cancel the operation if necessary.  You do not need to call Dispose 
        /// for any other reason.</returns>
        /// <param name="message">Message to send</param>
        /// <param name="sender">Optional sender override.  The sender is handled automatically if you do not provide one.</param>
        /// <param name="delayUntil">Date and time to send</param>
        public static IDisposable tellParent(object message, DateTime delayUntil, ProcessId sender = default(ProcessId)) =>
            tell(ActorContext.Parent, message, delayUntil);


        /// <summary>
        /// Send a message to ourself
        /// </summary>
        /// <param name="message">Message to send</param>
        /// <param name="sender">Optional sender override.  The sender is handled automatically if you do not provide one.</param>
        public static Unit tellSelf(object message) =>
            tell(ActorContext.Self, message);

        /// <summary>
        /// Send a message to ourself at a specified time in the future
        /// </summary>
        /// <returns>IDisposable that you can use to cancel the operation if necessary.  You do not need to call Dispose 
        /// for any other reason.</returns>
        /// <param name="message">Message to send</param>
        /// <param name="sender">Optional sender override.  The sender is handled automatically if you do not provide one.</param>
        /// <param name="delayFor">How long to delay sending for</param>
        public static IDisposable tellSelf(object message, TimeSpan delayFor, ProcessId sender = default(ProcessId)) =>
            tell(ActorContext.Self, message, delayFor);

        /// <summary>
        /// Send a message to ourself at a specified time in the future
        /// </summary>
        /// <remarks>
        /// This will fail to be accurate across a Daylight Saving Time boundary
        /// </remarks>
        /// <returns>IDisposable that you can use to cancel the operation if necessary.  You do not need to call Dispose 
        /// for any other reason.</returns>
        /// <param name="message">Message to send</param>
        /// <param name="sender">Optional sender override.  The sender is handled automatically if you do not provide one.</param>
        /// <param name="delayUntil">Date and time to send</param>
        public static IDisposable tellSelf(object message, DateTime delayUntil, ProcessId sender = default(ProcessId)) =>
            tell(ActorContext.Self, message, delayUntil);
    }
}