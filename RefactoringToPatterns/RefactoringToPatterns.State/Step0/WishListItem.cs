﻿using RefactoringToPatterns.State.Common;
using RefactoringToPatterns.State.Common.Enum;
using RefactoringToPatterns.State.Common.Exceptions.Permission;
using RefactoringToPatterns.State.Common.Exceptions.Status;

namespace RefactoringToPatterns.State.Step0
{
    public class WishListItem
    {
        private const decimal AdditionalAcceptanceCostAmount = 5000;
        private readonly bool _areCostsInvoiced;
        private readonly decimal _itemCost;
        private readonly User _owner;

        public WishListItem(
            WishListItemStatus status,
            User owner,
            decimal itemCost,
            bool areCostsInvoiced = true)
        {
            _owner = owner;
            Status = status;
            _itemCost = itemCost;
            _areCostsInvoiced = areCostsInvoiced;
        }

        public WishListItemStatus Status { get; private set; }

        public void AcceptBy(User user)
        {
            if (Status != WishListItemStatus.Requested &&
                Status != WishListItemStatus.RequestedToDirector)
            {
                throw new CannotAcceptWishListItemWithCurrentStatusException(Status);
            }

            if (Status == WishListItemStatus.Requested)
            {
                if (!user.IsLeaderOf(_owner))
                {
                    throw new UserDoesNotHavePermissionToAcceptRequestedWishListItemException();
                }

                if (ShouldBeRequestedToDirector())
                {
                    Status = WishListItemStatus.RequestedToDirector;
                }
                else
                {
                    Status = WishListItemStatus.Accepted;
                }

                return;
            }

            if (!user.IsDirectorOf(_owner))
            {
                throw new UserDoesNotHavePermissionToAcceptRequestedWishListItemException();
            }

            Status = WishListItemStatus.Accepted;
        }

        public void RejectBy(User user)
        {
            if (Status != WishListItemStatus.Requested &&
                Status != WishListItemStatus.RequestedToDirector)
            {
                throw new CannotRejectWishListItemWithCurrentStatusException(Status);
            }

            if (Status == WishListItemStatus.Requested)
            {
                if (!user.IsLeaderOf(_owner))
                {
                    throw new UserDoesNotHavePermissionToRejectRequestedWishListItemException();
                }

                Status = WishListItemStatus.Rejected;

                return;
            }

            if (!user.IsDirectorOf(_owner))
            {
                throw new UserDoesNotHavePermissionToRejectRequestedWishListItemException();
            }

            Status = WishListItemStatus.Rejected;
        }

        public void StartRealizationBy(User user)
        {
            if (Status != WishListItemStatus.Accepted)
            {
                throw new CannotStartWishListItemRealizationWithCurrentStatusException(Status);
            }

            if (!user.IsSupervisor)
            {
                throw new UserDoesNotHavePermissionToStartWishListItemRealizationException();
            }

            Status = WishListItemStatus.InRealization;
        }

        public void FinishRealizationBy(User user)
        {
            if (Status != WishListItemStatus.InRealization)
            {
                throw new CannotFinishWishListItemRealizationWithCurrentStatusException(Status);
            }

            if (!user.IsSupervisor)
            {
                throw new UserDoesNotHavePermissionToFinishWishListItemRealizationException();
            }

            if (!_areCostsInvoiced)
            {
                throw new CannotFinishWishListItemRealizationWithNotInvoicedException();
            }

            Status = WishListItemStatus.Realized;
        }

        private bool ShouldBeRequestedToDirector()
        {
            return _itemCost >= AdditionalAcceptanceCostAmount;
        }
    }
}