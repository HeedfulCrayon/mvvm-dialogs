﻿using System;

namespace MvvmDialogs
{
	/// <summary>
	/// Exception thrown by <see cref="IDialogService"/>
	/// </summary>
	/// <seealso cref="System.Exception" />
	public class ViewModelNotFoundException : Exception
	{
        /// <summary>
        /// Initializes a new instance of the <see cref="ViewModelNotFoundException"/> class.
        /// </summary>
        public ViewModelNotFoundException(Type viewModelType)
			: base(BuildMessage(viewModelType))
		{
		}

		private static string BuildMessage(Type viewModelType)
		{
			throw new NotImplementedException();
		}
	}
}