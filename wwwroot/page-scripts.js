"use strict";

const ticketForm = document.getElementById("ticket-form");

if (ticketForm) {
    ticketForm.addEventListener("submit", function (event) {
        event.preventDefault();
    });
}

const addMemberButton = document.getElementById("add-member");
const memberDialog = document.getElementById("member-dialog");
const memberForm = document.getElementById("member-form");

if (
    addMemberButton &&
    memberDialog instanceof HTMLDialogElement &&
    memberForm instanceof HTMLFormElement
) {
    addMemberButton.addEventListener("click", function () {
        memberForm.reset();
        memberDialog.showModal();
    });

    memberDialog
        .querySelectorAll("[data-close-member]")
        .forEach(function (button) {
            button.addEventListener("click", function () {
                memberDialog.close();
            });
        });

    memberForm.addEventListener("submit", function (event) {
        event.preventDefault();
    });
}

const createProjectButton = document.getElementById("create-project");
const projectDialog = document.getElementById("project-dialog");
const projectForm = document.getElementById("project-form");

if (
    createProjectButton &&
    projectDialog instanceof HTMLDialogElement &&
    projectForm instanceof HTMLFormElement
) {
    createProjectButton.addEventListener("click", function () {
        projectForm.reset();
        projectDialog.showModal();
    });

    projectDialog
        .querySelectorAll("[data-close-project]")
        .forEach(function (button) {
            button.addEventListener("click", function () {
                projectDialog.close();
            });
        });

    projectForm.addEventListener("submit", function (event) {
        event.preventDefault();
    });
}

const addSoftwareButton = document.getElementById("add-software");
const softwareDialog = document.getElementById("software-dialog");
const softwareForm = document.getElementById("software-form");
const softwareName = document.getElementById("software-name");
const softwareVersion = document.getElementById("software-version");
const softwareLink = document.getElementById("software-link");
const softwareTitle = document.getElementById("software-dialog-title");

if (
    addSoftwareButton &&
    softwareDialog instanceof HTMLDialogElement &&
    softwareForm instanceof HTMLFormElement &&
    softwareName instanceof HTMLInputElement &&
    softwareVersion instanceof HTMLInputElement &&
    softwareLink instanceof HTMLInputElement &&
    softwareTitle
) {
    const softwareSaveButton =
        softwareForm.querySelector(".button-primary");

    addSoftwareButton.addEventListener("click", function () {
        softwareForm.reset();

        softwareTitle.textContent = "Добавить ПО";

        if (softwareSaveButton) {
            softwareSaveButton.textContent = "Добавить ПО";
        }

        softwareDialog.showModal();
    });

    document
        .querySelectorAll("[data-edit-software]")
        .forEach(function (button) {
            button.addEventListener("click", function () {
                const card = button.closest(".software-card");

                if (!card) {
                    return;
                }

                const name = card.querySelector("h2");
                const version = card.querySelector(".version-badge");
                const link = card.querySelector(".software-link");

                softwareForm.reset();

                softwareName.value =
                    name?.textContent?.trim() || "";

                softwareVersion.value =
                    (version?.textContent || "")
                        .replace("Версия", "")
                        .trim();

                softwareLink.value =
                    link?.getAttribute("href") || "";

                softwareTitle.textContent = "Изменить ПО";

                if (softwareSaveButton) {
                    softwareSaveButton.textContent = "Сохранить";
                }

                softwareDialog.showModal();
            });
        });

    softwareDialog
        .querySelectorAll("[data-close-software]")
        .forEach(function (button) {
            button.addEventListener("click", function () {
                softwareDialog.close();
            });
        });

    softwareForm.addEventListener("submit", function (event) {
        event.preventDefault();
    });
}

const assignmentForm = document.getElementById("assignment-form");

if (assignmentForm instanceof HTMLFormElement) {
    assignmentForm.addEventListener("submit", function (event) {
        event.preventDefault();
    });
}

const reviewForm = document.getElementById("review-form");
const returnReason = document.getElementById("return-reason");
const returnError = document.getElementById("return-error");

if (
    reviewForm instanceof HTMLFormElement &&
    returnReason instanceof HTMLTextAreaElement &&
    returnError
) {
    reviewForm.addEventListener("submit", function (event) {
        event.preventDefault();

        if (!(event instanceof SubmitEvent)) {
            return;
        }

        const button = event.submitter;

        if (
            button instanceof HTMLButtonElement &&
            button.value === "return" &&
            returnReason.value.trim() === ""
        ) {
            returnError.hidden = false;
            returnReason.setAttribute("aria-invalid", "true");
            returnReason.focus();
            return;
        }

        returnError.hidden = true;
        returnReason.removeAttribute("aria-invalid");
    });

    returnReason.addEventListener("input", function () {
        if (returnReason.value.trim() !== "") {
            returnError.hidden = true;
            returnReason.removeAttribute("aria-invalid");
        }
    });
}